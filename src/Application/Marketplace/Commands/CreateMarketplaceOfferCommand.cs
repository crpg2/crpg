using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Models;
using Crpg.Domain.Entities.Marketplace;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Marketplace.Commands;

public record CreateMarketplaceOfferCommand : IMediatorRequest<MarketplaceOfferViewModel>
{
    private const int ActiveOfferLimit = 5;
    private static readonly TimeSpan OfferDuration = TimeSpan.FromDays(7);

    [JsonIgnore]
    public int UserId { get; set; }

    public List<MarketplaceOfferAssetInput> OfferedAssets { get; init; } = [];
    public List<MarketplaceOfferAssetInput> RequestedAssets { get; init; } = [];

    public class Validator : AbstractValidator<CreateMarketplaceOfferCommand>
    {
        public Validator()
        {
            RuleFor(c => c.OfferedAssets).NotEmpty();
            RuleFor(c => c.RequestedAssets).NotEmpty();
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<CreateMarketplaceOfferCommand, MarketplaceOfferViewModel>
    {
        private sealed record AssetValidationResult(MarketplaceOfferAsset? Asset, Error? Error);

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<MarketplaceOfferViewModel>> Handle(CreateMarketplaceOfferCommand req, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            await MarketplaceOfferLifecycle.ExpireActiveOffersForUserAsync(_db, req.UserId, now, cancellationToken);

            int activeOffersCount = await _db.MarketplaceOffers
                .CountAsync(o => o.SellerUserId == req.UserId && o.Status == MarketplaceOfferStatus.Active && o.ExpiresAt > now, cancellationToken);
            if (activeOffersCount >= ActiveOfferLimit)
            {
                return new(CommonErrors.MarketplaceOfferLimitReached(req.UserId, ActiveOfferLimit));
            }

            var seller = await _db.Users
                .Include(u => u.Items)
                    .ThenInclude(i => i.Item)
                .Include(u => u.Items)
                    .ThenInclude(i => i.EquippedItems)
                .Include(u => u.Items)
                    .ThenInclude(i => i.PersonalItem)
                .Include(u => u.Items)
                    .ThenInclude(i => i.ClanArmoryBorrowedItem)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (seller == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            var offerAssets = new List<MarketplaceOfferAsset>();

            int reservedGold = 0;
            int reservedHeirloomPoints = 0;

            foreach (var assetInput in req.OfferedAssets)
            {
                var validationResult = await ValidateAndBuildOfferedAssetAsync(assetInput, seller, cancellationToken);
                if (validationResult.Error != null)
                {
                    return new(validationResult.Error);
                }

                offerAssets.Add(validationResult.Asset!);
                if (assetInput.Type == MarketplaceAssetType.Gold)
                {
                    reservedGold += assetInput.Amount;
                }

                if (assetInput.Type == MarketplaceAssetType.HeirloomPoints)
                {
                    reservedHeirloomPoints += assetInput.Amount;
                }
            }

            foreach (var assetInput in req.RequestedAssets)
            {
                var validationResult = ValidateAndBuildRequestedAsset(assetInput);
                if (validationResult.Error != null)
                {
                    return new(validationResult.Error);
                }

                offerAssets.Add(validationResult.Asset!);
            }

            if (seller.Gold < reservedGold)
            {
                return new(CommonErrors.NotEnoughGold(reservedGold, seller.Gold));
            }

            if (seller.HeirloomPoints < reservedHeirloomPoints)
            {
                return new(CommonErrors.NotEnoughHeirloomPoints(reservedHeirloomPoints, seller.HeirloomPoints));
            }

            seller.Gold -= reservedGold;
            seller.HeirloomPoints -= reservedHeirloomPoints;

            var offer = new MarketplaceOffer
            {
                SellerUserId = req.UserId,
                Status = MarketplaceOfferStatus.Active,
                ExpiresAt = now.Add(OfferDuration),
                Assets = offerAssets,
            };

            _db.MarketplaceOffers.Add(offer);
            await _db.SaveChangesAsync(cancellationToken);

            return new(_mapper.Map<MarketplaceOfferViewModel>(offer));
        }

        private async Task<AssetValidationResult> ValidateAndBuildOfferedAssetAsync(
            MarketplaceOfferAssetInput input,
            Crpg.Domain.Entities.Users.User seller,
            CancellationToken cancellationToken)
        {
            if (input.Type == MarketplaceAssetType.Item)
            {
                if (input.UserItemId == null)
                {
                    return new(null, CommonErrors.MarketplaceOfferInvalidAsset("Offered item must provide userItemId"));
                }

                var userItem = seller.Items.FirstOrDefault(i => i.Id == input.UserItemId.Value);
                if (userItem == null)
                {
                    return new(null, CommonErrors.UserItemNotFound(input.UserItemId.Value));
                }

                if (userItem.PersonalItem != null || userItem.ClanArmoryBorrowedItem != null)
                {
                    return new(null, CommonErrors.MarketplaceOfferInvalidAsset($"User item '{userItem.Id}' cannot be traded"));
                }

                if (userItem.EquippedItems.Count > 0)
                {
                    return new(null, CommonErrors.UserItemInUse(userItem.Id));
                }

                bool alreadyListed = await _db.MarketplaceOfferAssets
                    .AnyAsync(a => a.UserItemId == userItem.Id
                        && a.Side == MarketplaceOfferAssetSide.Offered
                        && a.MarketplaceOffer != null
                        && a.MarketplaceOffer.Status == MarketplaceOfferStatus.Active
                        && a.MarketplaceOffer.ExpiresAt > DateTime.UtcNow,
                        cancellationToken);
                if (alreadyListed)
                {
                    return new(null, CommonErrors.MarketplaceOfferInvalidAsset($"User item '{userItem.Id}' is already listed"));
                }

                return new(new MarketplaceOfferAsset
                {
                    Side = MarketplaceOfferAssetSide.Offered,
                    Type = MarketplaceAssetType.Item,
                    Amount = 1,
                    ItemId = userItem.ItemId,
                    UserItemId = userItem.Id,
                    UserItem = userItem,
                }, null);
            }

            if (input.Amount <= 0)
            {
                return new(null, CommonErrors.MarketplaceOfferInvalidAsset("Amount must be greater than zero"));
            }

            if (input.UserItemId != null || input.ItemId != null)
            {
                return new(null, CommonErrors.MarketplaceOfferInvalidAsset("Currency assets cannot contain item references"));
            }

            return new(new MarketplaceOfferAsset
            {
                Side = MarketplaceOfferAssetSide.Offered,
                Type = input.Type,
                Amount = input.Amount,
            }, null);
        }

        private static AssetValidationResult ValidateAndBuildRequestedAsset(MarketplaceOfferAssetInput input)
        {
            if (input.Type == MarketplaceAssetType.Item)
            {
                if (string.IsNullOrWhiteSpace(input.ItemId) || input.Amount <= 0)
                {
                    return new(null, CommonErrors.MarketplaceOfferInvalidAsset("Requested item must provide itemId and amount"));
                }

                if (input.UserItemId != null)
                {
                    return new(null, CommonErrors.MarketplaceOfferInvalidAsset("Requested item cannot reference userItemId"));
                }

                return new(new MarketplaceOfferAsset
                {
                    Side = MarketplaceOfferAssetSide.Requested,
                    Type = MarketplaceAssetType.Item,
                    Amount = input.Amount,
                    ItemId = input.ItemId,
                }, null);
            }

            if (input.Amount <= 0)
            {
                return new(null, CommonErrors.MarketplaceOfferInvalidAsset("Amount must be greater than zero"));
            }

            if (input.UserItemId != null || input.ItemId != null)
            {
                return new(null, CommonErrors.MarketplaceOfferInvalidAsset("Currency assets cannot contain item references"));
            }

            return new(new MarketplaceOfferAsset
            {
                Side = MarketplaceOfferAssetSide.Requested,
                Type = input.Type,
                Amount = input.Amount,
            }, null);
        }
    }
}

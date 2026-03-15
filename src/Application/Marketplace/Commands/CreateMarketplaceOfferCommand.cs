using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Models;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Marketplace.Commands;

public record CreateMarketplaceOfferCommand : IMediatorRequest<MarketplaceOfferViewModel>
{
    [JsonIgnore]
    public int UserId { get; set; }

    public MarketplaceOfferAssetInput Offer { get; init; } = default!;
    public MarketplaceOfferAssetInput Request { get; init; } = default!;

    internal class Handler(ICrpgDbContext db, IMapper mapper, Constants constants, IActivityLogService activityLogService, IDateTime dateTime) : IMediatorRequestHandler<CreateMarketplaceOfferCommand, MarketplaceOfferViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreateMarketplaceOfferCommand>();
        private sealed record AssetValidationResult(MarketplaceOfferAsset? Asset, Error? Error);
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly Constants _constants = constants;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IDateTime _dateTime = dateTime;

        public async ValueTask<Result<MarketplaceOfferViewModel>> Handle(CreateMarketplaceOfferCommand req, CancellationToken cancellationToken)
        {
            var seller = await _db.Users
                .AsSplitQuery()
                .Include(u => u.Items)
                    .ThenInclude(i => i.Item)
                .Include(u => u.Items)
                    .ThenInclude(i => i.PersonalItem)
                .Include(u => u.Items)
                    .ThenInclude(i => i.ClanArmoryItem)
                .Include(u => u.Items)
                    .ThenInclude(i => i.ClanArmoryBorrowedItem)
                .Include(u => u.Offers.Where(o => o.ExpiresAt > _dateTime.UtcNow))
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (seller == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (seller.Offers.Count >= _constants.MarketplaceActiveOfferLimit)
            {
                return new(CommonErrors.MarketplaceOfferLimitReached(req.UserId, _constants.MarketplaceActiveOfferLimit));
            }

            var conflictingCurrencyError = ValidateNoCurrencyOverlap(req.Offer, req.Request);
            if (conflictingCurrencyError != null)
            {
                return new(conflictingCurrencyError);
            }

            var offerAssets = new List<MarketplaceOfferAsset>();

            var offerValidationResult = ValidateAndBuildOfferedAsset(req.Offer, seller);
            if (offerValidationResult.Error != null)
            {
                return new(offerValidationResult.Error);
            }

            var offeredAsset = offerValidationResult.Asset!;

            offerAssets.Add(offeredAsset);

            var requestValidationResult = await ValidateAndBuildRequestedAssetAsync(req.Request, cancellationToken);
            if (requestValidationResult.Error != null)
            {
                return new(requestValidationResult.Error);
            }

            var requestedAsset = requestValidationResult.Asset!;

            offerAssets.Add(requestedAsset);

            // Reserve resources from seller
            int listingFee = _constants.MarketplaceListingFeePerDay * _constants.MarketplaceOfferDurationDays;
            int goldFee = (int)((offeredAsset.Gold + requestedAsset.Gold) * _constants.MarketplaceGoldFeePercent / 100.0);

            int reservedGold = offeredAsset.Gold + goldFee;
            int reservedHeirloomPoints = offeredAsset.HeirloomPoints;

            if (seller.Gold < reservedGold + listingFee)
            {
                return new(CommonErrors.NotEnoughGold(reservedGold + listingFee, seller.Gold));
            }

            if (seller.HeirloomPoints < reservedHeirloomPoints)
            {
                return new(CommonErrors.NotEnoughHeirloomPoints(reservedHeirloomPoints, seller.HeirloomPoints));
            }

            seller.Gold -= listingFee;
            seller.Gold -= reservedGold;
            seller.HeirloomPoints -= reservedHeirloomPoints;

            var offer = new MarketplaceOffer
            {
                SellerId = req.UserId,
                Assets = offerAssets,
                ExpiresAt = _dateTime.UtcNow.AddDays(_constants.MarketplaceOfferDurationDays),
                GoldFee = goldFee,
            };

            _db.MarketplaceOffers.Add(offer);

            _db.ActivityLogs.Add(_activityLogService.CreateMarketplaceOfferCreatedLog(userId: req.UserId, offerId: offer.Id, listingFee: listingFee, goldFee: goldFee, offered: offeredAsset, requested: requestedAsset));

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' created marketplace offer '{1}'", req.UserId, offer.Id);
            return new(_mapper.Map<MarketplaceOfferViewModel>(offer));
        }

        private static Error? ValidateNoCurrencyOverlap(MarketplaceOfferAssetInput offerAsset, MarketplaceOfferAssetInput requestAsset)
        {
            if (offerAsset.Gold > 0 && requestAsset.Gold > 0)
            {
                return CommonErrors.MarketplaceOfferInvalidAsset("Gold cannot be both offered and requested in the same offer");
            }

            if (offerAsset.HeirloomPoints > 0 && requestAsset.HeirloomPoints > 0)
            {
                return CommonErrors.MarketplaceOfferInvalidAsset("Heirloom points cannot be both offered and requested in the same offer");
            }

            return null;
        }

        private static AssetValidationResult ValidateAndBuildOfferedAsset(MarketplaceOfferAssetInput input, User seller)
        {
            if (input.UserItemId == null && input.HeirloomPoints <= 0 && input.Gold <= 0)
            {
                return new(null, CommonErrors.MarketplaceOfferInvalidAsset("Offered asset must provide userItem or Gold/HeirloomPoints amount"));
            }

            var marketplaceOfferAsset = new MarketplaceOfferAsset()
            {
                Side = MarketplaceOfferAssetSide.Offered,
            };

            if (input.UserItemId != null)
            {
                var userItem = seller.Items.FirstOrDefault(i => i.Id == input.UserItemId);
                if (userItem == null)
                {
                    return new(null, CommonErrors.UserItemNotFound(input.UserItemId.Value));
                }

                if (userItem.IsBroken)
                {
                    return new(null, CommonErrors.MarketplaceOfferInvalidAsset($"User item '{userItem.Id}' is broken and cannot be traded"));
                }

                if (userItem.PersonalItem != null)
                {
                    return new(null, CommonErrors.MarketplaceOfferInvalidAsset($"User item '{userItem.Id}' is a personal item and cannot be traded"));
                }

                if (userItem.ClanArmoryItem != null)
                {
                    return new(null, CommonErrors.MarketplaceOfferInvalidAsset($"User item '{userItem.Id}' is in the clan armory and cannot be traded"));
                }

                if (userItem.ClanArmoryBorrowedItem != null)
                {
                    return new(null, CommonErrors.MarketplaceOfferInvalidAsset($"User item '{userItem.Id}' is borrowed from the clan armory and cannot be traded"));
                }

                marketplaceOfferAsset.UserItemId = userItem.Id;
            }

            if (input.Gold > 0)
            {
                marketplaceOfferAsset.Gold = input.Gold.Value;
            }

            if (input.HeirloomPoints > 0)
            {
                marketplaceOfferAsset.HeirloomPoints = input.HeirloomPoints.Value;
            }

            return new(marketplaceOfferAsset, null);
        }

        private async Task<AssetValidationResult> ValidateAndBuildRequestedAssetAsync(MarketplaceOfferAssetInput input, CancellationToken cancellationToken)
        {
            if (input.ItemId == null && input.HeirloomPoints <= 0 && input.Gold <= 0)
            {
                return new(null, CommonErrors.MarketplaceOfferInvalidAsset("Offered asset must provide Item or HeirloomPoints/Gold amount"));
            }

            var marketplaceOfferAsset = new MarketplaceOfferAsset()
            {
                Side = MarketplaceOfferAssetSide.Requested,
            };

            if (input.ItemId != null)
            {
                var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == input.ItemId, cancellationToken);
                if (item == null)
                {
                    return new(null, CommonErrors.ItemNotFound(input.ItemId));
                }

                marketplaceOfferAsset.ItemId = input.ItemId;
            }

            if (input.Gold > 0)
            {
                marketplaceOfferAsset.Gold = input.Gold.Value;
            }

            if (input.HeirloomPoints > 0)
            {
                marketplaceOfferAsset.HeirloomPoints = input.HeirloomPoints.Value;
            }

            return new(marketplaceOfferAsset, null);
        }
    }
}

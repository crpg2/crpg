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

public record CreateMarketplaceListingCommand : IMediatorRequest<MarketplaceListingViewModel>
{
    [JsonIgnore]
    public int UserId { get; set; }

    public MarketplaceListingAssetInput Offer { get; init; } = default!;
    public MarketplaceListingAssetInput Request { get; init; } = default!;

    internal class Handler(ICrpgDbContext db, IMapper mapper, Constants constants, IActivityLogService activityLogService, IDateTime dateTime) : IMediatorRequestHandler<CreateMarketplaceListingCommand, MarketplaceListingViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreateMarketplaceListingCommand>();
        private sealed record AssetValidationResult(MarketplaceListingAsset? Asset, Error? Error);
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly Constants _constants = constants;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IDateTime _dateTime = dateTime;

        public async ValueTask<Result<MarketplaceListingViewModel>> Handle(CreateMarketplaceListingCommand req, CancellationToken cancellationToken)
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
                .Include(u => u.MarketplaceListings.Where(l => l.ExpiresAt > _dateTime.UtcNow))
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (seller == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (seller.MarketplaceListings.Count >= _constants.MarketplaceActiveListingLimit)
            {
                return new(CommonErrors.MarketplaceListingLimitReached(req.UserId, _constants.MarketplaceActiveListingLimit));
            }

            var conflictingCurrencyError = ValidateNoCurrencyOverlap(req.Offer, req.Request);
            if (conflictingCurrencyError != null)
            {
                return new(conflictingCurrencyError);
            }

            var listingAssets = new List<MarketplaceListingAsset>();

            var offerValidationResult = ValidateAndBuildOfferAsset(req.Offer, seller);
            if (offerValidationResult.Error != null)
            {
                return new(offerValidationResult.Error);
            }

            var offerAsset = offerValidationResult.Asset!;

            listingAssets.Add(offerAsset);

            var requestValidationResult = await ValidateAndBuildRequestAssetAsync(req.Request, cancellationToken);
            if (requestValidationResult.Error != null)
            {
                return new(requestValidationResult.Error);
            }

            var requestAsset = requestValidationResult.Asset!;

            listingAssets.Add(requestAsset);

            // Reserve resources from seller
            int listingFee = _constants.MarketplaceListingFeePerDay * _constants.MarketplaceListingDurationDays;
            int goldFee = (int)((offerAsset.Gold + requestAsset.Gold) * _constants.MarketplaceGoldFeePercent / 100.0);

            int reservedGold = offerAsset.Gold + goldFee;
            int reservedHeirloomPoints = offerAsset.HeirloomPoints;

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

            var listing = new MarketplaceListing
            {
                SellerId = req.UserId,
                Assets = listingAssets,
                ExpiresAt = _dateTime.UtcNow.AddDays(_constants.MarketplaceListingDurationDays),
                GoldFee = goldFee,
            };

            _db.MarketplaceListings.Add(listing);

            _db.ActivityLogs.Add(_activityLogService.CreateMarketplaceListingCreatedLog(userId: req.UserId, listingId: listing.Id, listingFee: listingFee, goldFee: goldFee, offer: offerAsset, request: requestAsset));

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' created marketplace listing '{1}'", req.UserId, listing.Id);
            return new(_mapper.Map<MarketplaceListingViewModel>(listing));
        }

        private static Error? ValidateNoCurrencyOverlap(MarketplaceListingAssetInput offerAsset, MarketplaceListingAssetInput requestAsset)
        {
            if (offerAsset.Gold > 0 && requestAsset.Gold > 0)
            {
                return CommonErrors.MarketplaceListingInvalidAsset("Gold cannot be both offered and requested in the same listing");
            }

            if (offerAsset.HeirloomPoints > 0 && requestAsset.HeirloomPoints > 0)
            {
                return CommonErrors.MarketplaceListingInvalidAsset("Heirloom points cannot be both offered and requested in the same listing");
            }

            return null;
        }

        private static AssetValidationResult ValidateAndBuildOfferAsset(MarketplaceListingAssetInput input, User seller)
        {
            if (input.UserItemId == null && input.HeirloomPoints <= 0 && input.Gold <= 0)
            {
                return new(null, CommonErrors.MarketplaceListingInvalidAsset("Offered asset must provide userItem or Gold/HeirloomPoints amount"));
            }

            var offeredAsset = new MarketplaceListingAsset()
            {
                Side = MarketplaceListingAssetSide.Offered,
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
                    return new(null, CommonErrors.MarketplaceListingInvalidAsset($"User item '{userItem.Id}' is broken and cannot be traded"));
                }

                if (userItem.PersonalItem != null)
                {
                    return new(null, CommonErrors.MarketplaceListingInvalidAsset($"User item '{userItem.Id}' is a personal item and cannot be traded"));
                }

                if (userItem.ClanArmoryItem != null)
                {
                    return new(null, CommonErrors.MarketplaceListingInvalidAsset($"User item '{userItem.Id}' is in the clan armory and cannot be traded"));
                }

                if (userItem.ClanArmoryBorrowedItem != null)
                {
                    return new(null, CommonErrors.MarketplaceListingInvalidAsset($"User item '{userItem.Id}' is borrowed from the clan armory and cannot be traded"));
                }

                offeredAsset.UserItemId = userItem.Id;
            }

            if (input.Gold > 0)
            {
                offeredAsset.Gold = input.Gold.Value;
            }

            if (input.HeirloomPoints > 0)
            {
                offeredAsset.HeirloomPoints = input.HeirloomPoints.Value;
            }

            return new(offeredAsset, null);
        }

        private async Task<AssetValidationResult> ValidateAndBuildRequestAssetAsync(MarketplaceListingAssetInput input, CancellationToken cancellationToken)
        {
            if (input.ItemId == null && input.HeirloomPoints <= 0 && input.Gold <= 0)
            {
                return new(null, CommonErrors.MarketplaceListingInvalidAsset("Offered asset must provide Item or HeirloomPoints/Gold amount"));
            }

            var requestedAsset = new MarketplaceListingAsset()
            {
                Side = MarketplaceListingAssetSide.Requested,
            };

            if (input.ItemId != null)
            {
                var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == input.ItemId, cancellationToken);
                if (item == null)
                {
                    return new(null, CommonErrors.ItemNotFound(input.ItemId));
                }

                requestedAsset.ItemId = input.ItemId;
            }

            if (input.Gold > 0)
            {
                requestedAsset.Gold = input.Gold.Value;
            }

            if (input.HeirloomPoints > 0)
            {
                requestedAsset.HeirloomPoints = input.HeirloomPoints.Value;
            }

            return new(requestedAsset, null);
        }
    }
}

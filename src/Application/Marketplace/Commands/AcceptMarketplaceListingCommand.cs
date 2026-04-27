using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Marketplace.Commands;

public record AcceptMarketplaceListingCommand : IMediatorRequest
{
    [JsonIgnore]
    public int UserId { get; set; }

    [JsonIgnore]
    public int ListingId { get; set; }

    internal class Handler(ICrpgDbContext db, IActivityLogService activityLogService, IUserNotificationService userNotificationService, IDateTime dateTime, IMarketplaceService marketplaceService) : IMediatorRequestHandler<AcceptMarketplaceListingCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<AcceptMarketplaceListingCommand>();
        private readonly ICrpgDbContext _db = db;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IUserNotificationService _userNotificationService = userNotificationService;
        private readonly IDateTime _dateTime = dateTime;
        private readonly IMarketplaceService _marketplaceService = marketplaceService;

        public async ValueTask<Result> Handle(AcceptMarketplaceListingCommand req, CancellationToken cancellationToken)
        {
            var listing = await _db.MarketplaceListings
                .AsSplitQuery()
                .Include(l => l.Assets)
                .FirstOrDefaultAsync(l => l.Id == req.ListingId, cancellationToken);
            var (listingError, offerAsset, requestAsset) = ValidateListing(req.UserId, req.ListingId, listing);
            if (listingError != null)
            {
                return new Result(listingError);
            }

            var buyer = await LoadUserWithItemsAsync(req.UserId, cancellationToken);
            var seller = await LoadUserWithItemsAsync(listing!.SellerId, cancellationToken);

            var (sellerError, offeredUserItem) = ValidateSeller(listing!.SellerId, seller, offerAsset!);
            if (sellerError != null)
            {
                return new Result(sellerError);
            }

            var (buyerError, requestedUserItem) = ValidateBuyer(req.UserId, buyer, requestAsset!);
            if (buyerError != null)
            {
                return new Result(buyerError);
            }

            ExecuteTransfer(buyer!, seller!, offerAsset!, requestAsset!, offeredUserItem, requestedUserItem);

            _db.MarketplaceListings.Remove(listing!);
            RecordLogsAndNotifications(req.UserId, listing!, offerAsset!, requestAsset!);
            await InvalidateCascadingListingsAsync(listing!.Id, listing.SellerId, buyer!.Id, offeredUserItem, requestedUserItem, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Marketplace listing '{0}' accepted by user '{1}'", listing!.Id, req.UserId);
            return Result.NoErrors;
        }

        private (Error? error, MarketplaceListingAsset? offerAsset, MarketplaceListingAsset? requestAsset) ValidateListing(int userId, int listingId, MarketplaceListing? listing)
        {
            if (listing == null)
            {
                return (CommonErrors.MarketplaceListingNotFound(listingId), null, null);
            }

            if (listing.SellerId == userId)
            {
                return (CommonErrors.MarketplaceListingSelfAccept(listingId), null, null);
            }

            if (listing.ExpiresAt <= _dateTime.UtcNow)
            {
                return (CommonErrors.MarketplaceListingExpired(listingId), null, null);
            }

            var offerAsset = listing.Assets.FirstOrDefault(a => a.Side == MarketplaceListingAssetSide.Offered);
            var requestAsset = listing.Assets.FirstOrDefault(a => a.Side == MarketplaceListingAssetSide.Requested);
            if (offerAsset == null || requestAsset == null)
            {
                return (CommonErrors.MarketplaceListingInvalidAsset("Listing must contain both offered and requested assets"), null, null);
            }

            return (null, offerAsset, requestAsset);
        }

        private async Task<User?> LoadUserWithItemsAsync(int userId, CancellationToken cancellationToken)
        {
            return await _db.Users
                .AsSplitQuery()
                .Include(u => u.Items)
                    .ThenInclude(ui => ui.Item)
                .Include(u => u.Items)
                    .ThenInclude(i => i.EquippedItems)
                .Include(u => u.Items)
                    .ThenInclude(ui => ui.PersonalItem)
                .Include(u => u.Items)
                    .ThenInclude(ui => ui.ClanArmoryBorrowedItem)
                .Include(u => u.Items)
                    .ThenInclude(ui => ui.ClanArmoryItem)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        private static (Error? error, UserItem? offeredUserItem) ValidateSeller(int sellerId, User? seller, MarketplaceListingAsset offerAsset)
        {
            if (seller == null)
            {
                return (CommonErrors.UserNotFound(sellerId), null);
            }

            if (offerAsset.UserItemId is not int offeredUserItemId)
            {
                return (null, null);
            }

            var offeredUserItem = seller.Items.FirstOrDefault(i => i.Id == offeredUserItemId);
            if (offeredUserItem == null)
            {
                return (CommonErrors.MarketplaceListingInvalidAsset("Offered user item is no longer available"), null);
            }

            if (IsUserItemLocked(offeredUserItem))
            {
                return (CommonErrors.MarketplaceListingInvalidAsset($"User item '{offeredUserItem.Id}' cannot be traded"), null);
            }

            return (null, offeredUserItem);
        }

        private static (Error? error, UserItem? requestedUserItem) ValidateBuyer(int userId, User? buyer, MarketplaceListingAsset requestAsset)
        {
            if (buyer == null)
            {
                return (CommonErrors.UserNotFound(userId), null);
            }

            if (buyer.Gold < requestAsset.Gold)
            {
                return (CommonErrors.NotEnoughGold(requestAsset.Gold, buyer.Gold), null);
            }

            if (buyer.HeirloomPoints < requestAsset.HeirloomPoints)
            {
                return (CommonErrors.NotEnoughHeirloomPoints(requestAsset.HeirloomPoints, buyer.HeirloomPoints), null);
            }

            if (string.IsNullOrWhiteSpace(requestAsset.ItemId))
            {
                return (null, null);
            }

            var requestedUserItem = buyer.Items
                .FirstOrDefault(i => i.ItemId == requestAsset.ItemId && !IsUserItemLocked(i));
            if (requestedUserItem == null)
            {
                return (CommonErrors.MarketplaceListingInvalidAsset($"Buyer has insufficient item '{requestAsset.ItemId}'"), null);
            }

            return (null, requestedUserItem);
        }

        private static bool IsUserItemLocked(UserItem item) =>
            item.IsBroken
            || item.PersonalItem != null
            || item.ClanArmoryItem != null
            || item.ClanArmoryBorrowedItem != null;

        private void ExecuteTransfer(
            User buyer, User seller,
            MarketplaceListingAsset offerAsset, MarketplaceListingAsset requestAsset,
            UserItem? offeredUserItem, UserItem? requestedUserItem)
        {
            // Buyer pays what the seller requested
            buyer.Gold -= requestAsset.Gold;
            buyer.HeirloomPoints -= requestAsset.HeirloomPoints;

            // The seller had already paid for listing gold/hp and goldFee when he created the listing

            seller.Gold += requestAsset.Gold;
            seller.HeirloomPoints += requestAsset.HeirloomPoints;

            // Buyer receives the offered gold/HP (seller's funds were locked at listing creation)
            buyer.Gold += offerAsset.Gold;
            buyer.HeirloomPoints += offerAsset.HeirloomPoints;

            if (offeredUserItem != null)
            {
                // The item can be equipped when listed; remove equipment records on transfer.
                if (offeredUserItem.EquippedItems.Count > 0)
                {
                    _db.EquippedItems.RemoveRange(offeredUserItem.EquippedItems);
                }

                offeredUserItem.UserId = buyer.Id;
            }

            if (requestedUserItem != null)
            {
                if (requestedUserItem.EquippedItems.Count > 0)
                {
                    _db.EquippedItems.RemoveRange(requestedUserItem.EquippedItems);
                }

                requestedUserItem.UserId = seller.Id;
            }
        }

        private void RecordLogsAndNotifications(
            int buyerId, MarketplaceListing listing,
            MarketplaceListingAsset offerAsset, MarketplaceListingAsset requestAsset)
        {
            _db.ActivityLogs.Add(_activityLogService.CreateMarketplaceListingAcceptedLog(
                buyerId: buyerId, sellerId: listing.SellerId, listingId: listing.Id, goldFee: listing.GoldFee, offer: offerAsset, request: requestAsset));
            _db.UserNotifications.Add(_userNotificationService.CreateMarketplaceListingAcceptedToSellerNotification(
                userId: listing.SellerId, buyerId: buyerId, listingId: listing.Id, goldFee: listing.GoldFee, offer: offerAsset, request: requestAsset));
            _db.UserNotifications.Add(_userNotificationService.CreateMarketplaceListingAcceptedToBuyerNotification(
                userId: buyerId, sellerId: listing.SellerId, listingId: listing.Id, offer: offerAsset, request: requestAsset));
        }

        private async Task InvalidateCascadingListingsAsync(
            int listingId, int sellerId, int buyerId,
            UserItem? offeredUserItem, UserItem? requestedUserItem,
            CancellationToken cancellationToken)
        {
            if (requestedUserItem != null)
            {
                await _marketplaceService.InvalidateListingsByUserItemIdAsync(_db, _activityLogService, _userNotificationService,
                    sellerId: buyerId, userItemId: requestedUserItem.Id, excludeListingId: listingId, cancellationToken);
            }

            if (offeredUserItem != null)
            {
                await _marketplaceService.InvalidateListingsByUserItemIdAsync(_db, _activityLogService, _userNotificationService,
                    sellerId: sellerId, userItemId: offeredUserItem.Id, excludeListingId: listingId, cancellationToken);
            }
        }
    }
}

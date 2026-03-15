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

public record AcceptMarketplaceOfferCommand : IMediatorRequest
{
    [JsonIgnore]
    public int UserId { get; set; }

    [JsonIgnore]
    public int OfferId { get; set; }

    internal class Handler(ICrpgDbContext db, IActivityLogService activityLogService, IUserNotificationService userNotificationService, IDateTime dateTime, IMarketplaceService marketplaceService) : IMediatorRequestHandler<AcceptMarketplaceOfferCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<AcceptMarketplaceOfferCommand>();
        private readonly ICrpgDbContext _db = db;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IUserNotificationService _userNotificationService = userNotificationService;
        private readonly IDateTime _dateTime = dateTime;
        private readonly IMarketplaceService _marketplaceService = marketplaceService;

        public async ValueTask<Result> Handle(AcceptMarketplaceOfferCommand req, CancellationToken cancellationToken)
        {
            var offer = await _db.MarketplaceOffers
                .AsSplitQuery()
                .Include(o => o.Assets)
                .FirstOrDefaultAsync(o => o.Id == req.OfferId, cancellationToken);
            var (offerError, offerAsset, requestAsset) = ValidateOffer(req.UserId, req.OfferId, offer);
            if (offerError != null)
            {
                return new Result(offerError);
            }

            var buyer = await LoadUserWithItemsAsync(req.UserId, cancellationToken);
            var seller = await LoadUserWithItemsAsync(offer!.SellerId, cancellationToken);

            var (sellerError, offeredUserItem) = ValidateSeller(offer!.SellerId, seller, offerAsset!);
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

            _db.MarketplaceOffers.Remove(offer!);
            RecordLogsAndNotifications(req.UserId, offer!, offerAsset!, requestAsset!);
            await InvalidateCascadingOffersAsync(offer!.Id, offer.SellerId, buyer!.Id, offeredUserItem, requestedUserItem, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Marketplace offer '{0}' accepted by user '{1}'", offer.Id, req.UserId);
            return Result.NoErrors;
        }

        private (Error? error, MarketplaceOfferAsset? offerAsset, MarketplaceOfferAsset? requestAsset) ValidateOffer(int userId, int offerId, MarketplaceOffer? offer)
        {
            if (offer == null)
            {
                return (CommonErrors.MarketplaceOfferNotFound(offerId), null, null);
            }

            if (offer.SellerId == userId)
            {
                return (CommonErrors.MarketplaceOfferSelfAccept(offerId), null, null);
            }

            if (offer.ExpiresAt <= _dateTime.UtcNow)
            {
                return (CommonErrors.MarketplaceOfferExpired(offerId), null, null);
            }

            var offerAsset = offer.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Offered);
            var requestAsset = offer.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Requested);
            if (offerAsset == null || requestAsset == null)
            {
                return (CommonErrors.MarketplaceOfferInvalidAsset("Offer must contain both offered and requested assets"), null, null);
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

        private static (Error? error, UserItem? offeredUserItem) ValidateSeller(int sellerId, User? seller, MarketplaceOfferAsset offerAsset)
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
                return (CommonErrors.MarketplaceOfferInvalidAsset("Offered user item is no longer available"), null);
            }

            if (IsUserItemLocked(offeredUserItem))
            {
                return (CommonErrors.MarketplaceOfferInvalidAsset($"User item '{offeredUserItem.Id}' cannot be traded"), null);
            }

            return (null, offeredUserItem);
        }

        private static (Error? error, UserItem? requestedUserItem) ValidateBuyer(int userId, User? buyer, MarketplaceOfferAsset requestAsset)
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
                return (CommonErrors.MarketplaceOfferInvalidAsset($"Buyer has insufficient item '{requestAsset.ItemId}'"), null);
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
            MarketplaceOfferAsset offerAsset, MarketplaceOfferAsset requestAsset,
            UserItem? offeredUserItem, UserItem? requestedUserItem)
        {
            // Buyer pays what the seller requested
            buyer.Gold -= requestAsset.Gold;
            buyer.HeirloomPoints -= requestAsset.HeirloomPoints;

            // The seller had already paid for offer gold/hp and goldFee when he created the offer

            seller.Gold += requestAsset.Gold;
            seller.HeirloomPoints += requestAsset.HeirloomPoints;

            // Buyer receives the offered gold/HP (seller's funds were locked at offer creation)
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
            int buyerId, MarketplaceOffer offer,
            MarketplaceOfferAsset offerAsset, MarketplaceOfferAsset requestAsset)
        {
            _db.ActivityLogs.Add(_activityLogService.CreateMarketplaceOfferAcceptedLog(
                buyerId: buyerId, sellerId: offer.SellerId, offerId: offer.Id, goldFee: offer.GoldFee,
                offered: offerAsset, requested: requestAsset));
            _db.UserNotifications.Add(_userNotificationService.CreateMarketplaceOfferAcceptedToSellerNotification(
                userId: offer.SellerId, buyerId: buyerId, offerId: offer.Id, goldFee: offer.GoldFee,
                offered: offerAsset, requested: requestAsset));
            _db.UserNotifications.Add(_userNotificationService.CreateMarketplaceOfferAcceptedToBuyerNotification(
                userId: buyerId, sellerId: offer.SellerId, offerId: offer.Id,
                offered: offerAsset, requested: requestAsset));
        }

        private async Task InvalidateCascadingOffersAsync(
            int offerId, int sellerId, int buyerId,
            UserItem? offeredUserItem, UserItem? requestedUserItem,
            CancellationToken cancellationToken)
        {
            if (requestedUserItem != null)
            {
                await _marketplaceService.InvalidateOffersByUserItemIdAsync(_db, _activityLogService, _userNotificationService,
                    sellerId: buyerId, userItemId: requestedUserItem.Id, excludeOfferId: offerId, cancellationToken);
            }

            if (offeredUserItem != null)
            {
                await _marketplaceService.InvalidateOffersByUserItemIdAsync(_db, _activityLogService, _userNotificationService,
                    sellerId: sellerId, userItemId: offeredUserItem.Id, excludeOfferId: offerId, cancellationToken);
            }
        }
    }
}

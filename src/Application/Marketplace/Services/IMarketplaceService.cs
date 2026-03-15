using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Marketplace.Services;

internal interface IMarketplaceService
{
    /// <summary>
    /// Refunds the seller's reserved resources (gold, gold fee, heirloom points) from the offer's offered asset
    /// and removes the offer. <see cref="MarketplaceOffer.Assets"/> and <see cref="MarketplaceOffer.Seller"/> must be loaded.
    /// </summary>
    void RefundAndRemoveOffer(ICrpgDbContext db, MarketplaceOffer offer);

    /// <summary>
    /// Invalidates all offers where the given item appears on either side (offered UserItem or requested item by id).
    /// Refunds the seller, writes activity logs and user notifications for each invalidated offer.
    /// </summary>
    Task InvalidateOffersByItemIdAsync(
        ICrpgDbContext db,
        IActivityLogService activityLogService,
        IUserNotificationService userNotificationService,
        string itemId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Invalidates all offers from <paramref name="sellerId"/> where the given user item is on the Offered side,
    /// excluding <paramref name="excludeOfferId"/>.
    /// Refunds the seller, writes activity logs and user notifications for each invalidated offer.
    /// </summary>
    Task InvalidateOffersByUserItemIdAsync(
        ICrpgDbContext db,
        IActivityLogService activityLogService,
        IUserNotificationService userNotificationService,
        int sellerId,
        int userItemId,
        int excludeOfferId,
        CancellationToken cancellationToken);
}

internal class MarketplaceService : IMarketplaceService
{
    public void RefundAndRemoveOffer(ICrpgDbContext db, MarketplaceOffer offer)
    {
        var offeredAsset = offer.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Offered)!;
        offer.Seller!.Gold += offeredAsset.Gold + offer.GoldFee;
        offer.Seller!.HeirloomPoints += offeredAsset.HeirloomPoints;

        db.MarketplaceOffers.Remove(offer);
    }

    public async Task InvalidateOffersByItemIdAsync(
        ICrpgDbContext db,
        IActivityLogService activityLogService,
        IUserNotificationService userNotificationService,
        string itemId,
        CancellationToken cancellationToken)
    {
        var offers = await db.MarketplaceOffers
            .AsSplitQuery()
            .Include(o => o.Assets)
                .ThenInclude(a => a.UserItem)
            .Include(o => o.Assets)
                .ThenInclude(a => a!.Item)
            .Include(o => o.Seller)
            .Where(o => o.Assets.Any(a =>
                (a.Side == MarketplaceOfferAssetSide.Offered && a.UserItem!.ItemId == itemId)
                || (a.Side == MarketplaceOfferAssetSide.Requested && a.ItemId == itemId)))
            .ToListAsync(cancellationToken);

        foreach (var offer in offers)
        {
            InvalidateOffer(db, activityLogService, userNotificationService, offer);
        }
    }

    public async Task InvalidateOffersByUserItemIdAsync(
        ICrpgDbContext db,
        IActivityLogService activityLogService,
        IUserNotificationService userNotificationService,
        int sellerId,
        int userItemId,
        int excludeOfferId,
        CancellationToken cancellationToken)
    {
        var offers = await db.MarketplaceOffers
            .AsSplitQuery()
            .Include(o => o.Assets)
                .ThenInclude(a => a.UserItem)
                    .ThenInclude(ui => ui!.Item)
            .Include(o => o.Assets)
                .ThenInclude(a => a.Item)
            .Include(o => o.Seller)
            .Where(o => o.SellerId == sellerId
                && o.Id != excludeOfferId
                && o.Assets.Any(a => a.Side == MarketplaceOfferAssetSide.Offered
                    && a.UserItemId == userItemId))
            .ToListAsync(cancellationToken);

        foreach (var offer in offers)
        {
            InvalidateOffer(db, activityLogService, userNotificationService, offer);
        }
    }

    private void InvalidateOffer(
        ICrpgDbContext db,
        IActivityLogService activityLogService,
        IUserNotificationService userNotificationService,
        MarketplaceOffer offer)
    {
        RefundAndRemoveOffer(db, offer);

        var offeredAsset = offer.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Offered)!;
        var requestedAsset = offer.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Requested)!;

        db.ActivityLogs.Add(activityLogService.CreateMarketplaceOfferInvalidatedLog(
            userId: offer.SellerId, offerId: offer.Id, goldFee: offer.GoldFee, offered: offeredAsset, requested: requestedAsset));
        db.UserNotifications.Add(userNotificationService.CreateMarketplaceOfferInvalidatedNotification(
            userId: offer.SellerId, offerId: offer.Id, goldFee: offer.GoldFee, offered: offeredAsset, requested: requestedAsset));
    }
}

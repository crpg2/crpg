using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Marketplace.Services;

internal interface IMarketplaceService
{
    /// <summary>
    /// Refunds the seller's reserved resources (gold, gold fee, heirloom points) from the listing's offered asset
    /// and removes the listing.
    /// </summary>
    void RefundAndRemoveListing(ICrpgDbContext db, MarketplaceListing listing);

    /// <summary>
    /// Invalidates all listings where the given item appears on either side (offered UserItem or requested item by id).
    /// Refunds the seller, writes activity logs and user notifications for each invalidated listing.
    /// </summary>
    Task InvalidateListingsByItemIdAsync(
        ICrpgDbContext db,
        IActivityLogService activityLogService,
        IUserNotificationService userNotificationService,
        string itemId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Invalidates all listings from <paramref name="sellerId"/> where the given user item is on the <see cref="MarketplaceListingAssetSide.Offered"/> side, excluding <paramref name="excludeListingId"/>.
    /// </summary>
    Task InvalidateListingsByUserItemIdAsync(
        ICrpgDbContext db,
        IActivityLogService activityLogService,
        IUserNotificationService userNotificationService,
        int sellerId,
        int userItemId,
        int excludeListingId,
        CancellationToken cancellationToken);
}

internal class MarketplaceService : IMarketplaceService
{
    public void RefundAndRemoveListing(ICrpgDbContext db, MarketplaceListing listing)
    {
        var offeredAsset = listing.Assets.FirstOrDefault(a => a.Side == MarketplaceListingAssetSide.Offered)!;
        listing.Seller!.Gold += offeredAsset.Gold + listing.GoldFee;
        listing.Seller!.HeirloomPoints += offeredAsset.HeirloomPoints;

        db.MarketplaceListings.Remove(listing);
    }

    public async Task InvalidateListingsByItemIdAsync(
        ICrpgDbContext db,
        IActivityLogService activityLogService,
        IUserNotificationService userNotificationService,
        string itemId,
        CancellationToken cancellationToken)
    {
        var listings = await db.MarketplaceListings
            .AsSplitQuery()
            .Include(o => o.Assets)
                .ThenInclude(a => a.UserItem)
            .Include(o => o.Assets)
                .ThenInclude(a => a!.Item)
            .Include(o => o.Seller)
            .Where(o => o.Assets.Any(a =>
                (a.Side == MarketplaceListingAssetSide.Offered && a.UserItem!.ItemId == itemId)
                || (a.Side == MarketplaceListingAssetSide.Requested && a.ItemId == itemId)))
            .ToListAsync(cancellationToken);

        foreach (var listing in listings)
        {
            InvalidateListing(db, activityLogService, userNotificationService, listing);
        }
    }

    public async Task InvalidateListingsByUserItemIdAsync(
        ICrpgDbContext db,
        IActivityLogService activityLogService,
        IUserNotificationService userNotificationService,
        int sellerId,
        int userItemId,
        int excludeListingId,
        CancellationToken cancellationToken)
    {
        var listings = await db.MarketplaceListings
            .AsSplitQuery()
            .Include(o => o.Assets)
                .ThenInclude(a => a.UserItem)
                    .ThenInclude(ui => ui!.Item)
            .Include(o => o.Assets)
                .ThenInclude(a => a.Item)
            .Include(o => o.Seller)
            .Where(o => o.SellerId == sellerId
                && o.Id != excludeListingId
                && o.Assets.Any(a => a.Side == MarketplaceListingAssetSide.Offered
                    && a.UserItemId == userItemId))
            .ToListAsync(cancellationToken);

        foreach (var listing in listings)
        {
            InvalidateListing(db, activityLogService, userNotificationService, listing);
        }
    }

    private void InvalidateListing(
        ICrpgDbContext db,
        IActivityLogService activityLogService,
        IUserNotificationService userNotificationService,
        MarketplaceListing listing)
    {
        RefundAndRemoveListing(db, listing);

        var offerAsset = listing.Assets.FirstOrDefault(a => a.Side == MarketplaceListingAssetSide.Offered)!;
        var requestAsset = listing.Assets.FirstOrDefault(a => a.Side == MarketplaceListingAssetSide.Requested)!;

        db.ActivityLogs.Add(activityLogService.CreateMarketplaceListingInvalidatedLog(
            userId: listing.SellerId, listingId: listing.Id, goldFee: listing.GoldFee, offer: offerAsset, request: requestAsset));
        db.UserNotifications.Add(userNotificationService.CreateMarketplaceListingInvalidatedNotification(
            userId: listing.SellerId, listingId: listing.Id, goldFee: listing.GoldFee, offer: offerAsset, request: requestAsset));
    }
}

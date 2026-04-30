using Crpg.Domain.Entities.Marketplace;

namespace Crpg.Application.UTest.Marketplace;

internal static class MarketplaceListingFactory
{
    internal static MarketplaceListing CreateListing(
        int sellerId,
        int goldFee = 0,
        int offeredGold = 0,
        int requestedGold = 0,
        int offeredHeirloomPoints = 0,
        int requestedHeirloomPoints = 0,
        DateTime? createdAt = null,
        DateTime? expiresAt = null,
        int? offeredUserItemId = null,
        string? requestedItemId = null)
    {
        DateTime created = createdAt ?? DateTime.UtcNow;
        return new MarketplaceListing
        {
            SellerId = sellerId,
            CreatedAt = created,
            ExpiresAt = expiresAt ?? created.AddDays(7),
            UpdatedAt = created,
            GoldFee = goldFee,
            Assets =
            [
                new MarketplaceListingAsset
                {
                    Side = MarketplaceListingAssetSide.Offered,
                    Gold = offeredGold,
                    HeirloomPoints = offeredHeirloomPoints,
                    UserItemId = offeredUserItemId,
                },
                new MarketplaceListingAsset
                {
                    Side = MarketplaceListingAssetSide.Requested,
                    Gold = requestedGold,
                    HeirloomPoints = requestedHeirloomPoints,
                    ItemId = requestedItemId,
                },
            ],
        };
    }
}

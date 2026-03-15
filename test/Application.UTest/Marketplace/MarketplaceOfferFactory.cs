using Crpg.Domain.Entities.Marketplace;

namespace Crpg.Application.UTest.Marketplace;

internal static class MarketplaceOfferFactory
{
    internal static MarketplaceOffer CreateOffer(
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
        return new MarketplaceOffer
        {
            SellerId = sellerId,
            CreatedAt = created,
            ExpiresAt = expiresAt ?? created.AddDays(7),
            UpdatedAt = created,
            GoldFee = goldFee,
            Assets =
            [
                new MarketplaceOfferAsset
                {
                    Side = MarketplaceOfferAssetSide.Offered,
                    Gold = offeredGold,
                    HeirloomPoints = offeredHeirloomPoints,
                    UserItemId = offeredUserItemId,
                },
                new MarketplaceOfferAsset
                {
                    Side = MarketplaceOfferAssetSide.Requested,
                    Gold = requestedGold,
                    HeirloomPoints = requestedHeirloomPoints,
                    ItemId = requestedItemId,
                },
            ],
        };
    }
}

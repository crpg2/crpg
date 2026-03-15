using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Marketplace;

internal static class MarketplaceOfferLifecycle
{
    public static async Task ExpireActiveOffersForUserAsync(ICrpgDbContext db, int userId, DateTime now, CancellationToken cancellationToken)
    {
        var expiringOffers = await db.MarketplaceOffers
            .Include(o => o.Assets)
            .Where(o => o.SellerUserId == userId && o.Status == MarketplaceOfferStatus.Active && o.ExpiresAt <= now)
            .ToListAsync(cancellationToken);

        if (expiringOffers.Count == 0)
        {
            return;
        }

        var seller = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (seller == null)
        {
            return;
        }

        foreach (var offer in expiringOffers)
        {
            offer.Status = MarketplaceOfferStatus.Expired;
            offer.ClosedAt = now;

            foreach (var asset in offer.Assets.Where(a => a.Side == MarketplaceOfferAssetSide.Offered))
            {
                switch (asset.Type)
                {
                    case MarketplaceAssetType.Gold:
                        seller.Gold += asset.Amount;
                        break;
                    case MarketplaceAssetType.HeirloomPoints:
                        seller.HeirloomPoints += asset.Amount;
                        break;
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public static void ExpireOfferWithRefund(Crpg.Domain.Entities.Users.User seller, MarketplaceOffer offer, DateTime now)
    {
        if (offer.Status != MarketplaceOfferStatus.Active || offer.ExpiresAt > now)
        {
            return;
        }

        offer.Status = MarketplaceOfferStatus.Expired;
        offer.ClosedAt = now;

        foreach (var asset in offer.Assets.Where(a => a.Side == MarketplaceOfferAssetSide.Offered))
        {
            switch (asset.Type)
            {
                case MarketplaceAssetType.Gold:
                    seller.Gold += asset.Amount;
                    break;
                case MarketplaceAssetType.HeirloomPoints:
                    seller.HeirloomPoints += asset.Amount;
                    break;
            }
        }
    }
}

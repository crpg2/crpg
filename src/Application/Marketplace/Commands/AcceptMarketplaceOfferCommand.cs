using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Models;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Marketplace.Commands;

public record AcceptMarketplaceOfferCommand : IMediatorRequest<MarketplaceOfferViewModel>
{
    [JsonIgnore]
    public int UserId { get; set; }

    [JsonIgnore]
    public int OfferId { get; set; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<AcceptMarketplaceOfferCommand, MarketplaceOfferViewModel>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<MarketplaceOfferViewModel>> Handle(AcceptMarketplaceOfferCommand req, CancellationToken cancellationToken)
        {
            var offer = await _db.MarketplaceOffers
                .Include(o => o.Assets)
                .FirstOrDefaultAsync(o => o.Id == req.OfferId, cancellationToken);
            if (offer == null)
            {
                return new(CommonErrors.MarketplaceOfferNotFound(req.OfferId));
            }

            if (offer.SellerUserId == req.UserId)
            {
                return new(CommonErrors.MarketplaceOfferSelfAccept(req.OfferId));
            }

            var seller = await _db.Users.FirstOrDefaultAsync(u => u.Id == offer.SellerUserId, cancellationToken);
            if (seller == null)
            {
                return new(CommonErrors.UserNotFound(offer.SellerUserId));
            }

            var buyer = await _db.Users
                .Include(u => u.Items)
                    .ThenInclude(i => i.EquippedItems)
                .Include(u => u.Items)
                    .ThenInclude(i => i.PersonalItem)
                .Include(u => u.Items)
                    .ThenInclude(i => i.ClanArmoryBorrowedItem)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (buyer == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            DateTime now = DateTime.UtcNow;
            MarketplaceOfferLifecycle.ExpireOfferWithRefund(seller, offer, now);
            if (offer.Status == MarketplaceOfferStatus.Expired)
            {
                await _db.SaveChangesAsync(cancellationToken);
                return new(CommonErrors.MarketplaceOfferExpired(req.OfferId));
            }

            if (offer.Status != MarketplaceOfferStatus.Active)
            {
                return new(CommonErrors.MarketplaceOfferInvalidStatus(req.OfferId, offer.Status));
            }

            int requestedGold = offer.Assets
                .Where(a => a.Side == MarketplaceOfferAssetSide.Requested && a.Type == MarketplaceAssetType.Gold)
                .Sum(a => a.Amount);
            int requestedHeirloomPoints = offer.Assets
                .Where(a => a.Side == MarketplaceOfferAssetSide.Requested && a.Type == MarketplaceAssetType.HeirloomPoints)
                .Sum(a => a.Amount);

            if (buyer.Gold < requestedGold)
            {
                return new(CommonErrors.NotEnoughGold(requestedGold, buyer.Gold));
            }

            if (buyer.HeirloomPoints < requestedHeirloomPoints)
            {
                return new(CommonErrors.NotEnoughHeirloomPoints(requestedHeirloomPoints, buyer.HeirloomPoints));
            }

            Dictionary<string, int> requestedItemsById = offer.Assets
                .Where(a => a.Side == MarketplaceOfferAssetSide.Requested
                    && a.Type == MarketplaceAssetType.Item
                    && !string.IsNullOrWhiteSpace(a.ItemId))
                .GroupBy(a => a.ItemId!)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

            int[] offeredItemIds = offer.Assets
                .Where(a => a.Side == MarketplaceOfferAssetSide.Offered && a.Type == MarketplaceAssetType.Item && a.UserItemId != null)
                .Select(a => a.UserItemId!.Value)
                .ToArray();

            var offeredItems = await _db.UserItems
                .Where(ui => offeredItemIds.Contains(ui.Id))
                .ToDictionaryAsync(ui => ui.Id, cancellationToken);

            if (offeredItems.Count != offeredItemIds.Length || offeredItems.Values.Any(i => i.UserId != seller.Id))
            {
                return new(CommonErrors.MarketplaceOfferInvalidAsset("One or more offered items are no longer available"));
            }

            var activeBuyerListings = await _db.MarketplaceOfferAssets
                .Where(a => a.Side == MarketplaceOfferAssetSide.Offered
                    && a.UserItemId != null
                    && a.MarketplaceOffer != null
                    && a.MarketplaceOffer.Status == MarketplaceOfferStatus.Active
                    && a.MarketplaceOffer.ExpiresAt > now
                    && a.MarketplaceOffer.SellerUserId == buyer.Id)
                .Select(a => a.UserItemId!.Value)
                .ToListAsync(cancellationToken);

            var selectedBuyerItems = new List<UserItem>();
            foreach (var (itemId, requiredCount) in requestedItemsById)
            {
                var candidates = buyer.Items
                    .Where(i => i.ItemId == itemId
                        && i.PersonalItem == null
                        && i.ClanArmoryBorrowedItem == null
                        && i.EquippedItems.Count == 0
                        && !activeBuyerListings.Contains(i.Id))
                    .Take(requiredCount)
                    .ToList();

                if (candidates.Count < requiredCount)
                {
                    return new(CommonErrors.MarketplaceOfferInvalidAsset($"Buyer has insufficient items '{itemId}' ({requiredCount} required)"));
                }

                selectedBuyerItems.AddRange(candidates);
            }

            buyer.Gold -= requestedGold;
            buyer.HeirloomPoints -= requestedHeirloomPoints;
            seller.Gold += requestedGold;
            seller.HeirloomPoints += requestedHeirloomPoints;

            int offeredGold = offer.Assets
                .Where(a => a.Side == MarketplaceOfferAssetSide.Offered && a.Type == MarketplaceAssetType.Gold)
                .Sum(a => a.Amount);
            int offeredHeirloom = offer.Assets
                .Where(a => a.Side == MarketplaceOfferAssetSide.Offered && a.Type == MarketplaceAssetType.HeirloomPoints)
                .Sum(a => a.Amount);
            buyer.Gold += offeredGold;
            buyer.HeirloomPoints += offeredHeirloom;

            foreach (var offeredItem in offeredItems.Values)
            {
                offeredItem.UserId = buyer.Id;
            }

            foreach (var requestedItem in selectedBuyerItems)
            {
                requestedItem.UserId = seller.Id;
            }

            offer.Status = MarketplaceOfferStatus.Completed;
            offer.BuyerUserId = buyer.Id;
            offer.ClosedAt = now;

            await _db.SaveChangesAsync(cancellationToken);

            return new(_mapper.Map<MarketplaceOfferViewModel>(offer));
        }
    }
}

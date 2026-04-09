using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Models;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Marketplace.Queries;

public record GetMarketplaceOffersQuery : IMediatorRequest<MarketplaceOffersPageViewModel>
{
    public record SideFilter
    {
        public string? ItemId { get; init; }
        public IList<int>? ItemRanks { get; init; }
        public ItemType? ItemType { get; init; }
        public MarketplaceCurrencyFilter Gold { get; init; } = default;
        public MarketplaceCurrencyFilter HeirloomPoints { get; init; } = default;
    }

    public SideFilter Offered { get; init; } = new();
    public SideFilter Requested { get; init; } = new();
    public int? SellerId { get; init; }
    public int? BuyerId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 15;

    internal class Handler(ICrpgDbContext db, IMapper mapper, IDateTime dateTime) : IMediatorRequestHandler<GetMarketplaceOffersQuery, MarketplaceOffersPageViewModel>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IDateTime _dateTime = dateTime;

        public async ValueTask<Result<MarketplaceOffersPageViewModel>> Handle(GetMarketplaceOffersQuery req, CancellationToken cancellationToken)
        {
            int page = Math.Max(1, req.Page);
            int pageSize = Math.Clamp(req.PageSize, 1, 100);

            IQueryable<MarketplaceOffer> query = _db.MarketplaceOffers
                .AsNoTracking()
                .Where(o => o.ExpiresAt > _dateTime.UtcNow);

            // "Show my offers" mode: only filter by seller, ignore everything else
            if (req.SellerId.HasValue)
            {
                query = query.Where(o => o.SellerId == req.SellerId.Value);
            }
            else
            {
                query = ApplySideFilters(query, MarketplaceOfferAssetSide.Offered, req.Offered);
                query = ApplySideFilters(query, MarketplaceOfferAssetSide.Requested, req.Requested);

                // "Only affordable" mode: applied after side filters
                if (req.BuyerId.HasValue)
                {
                    var buyer = await _db.Users
                        .AsNoTracking()
                        .Where(u => u.Id == req.BuyerId.Value)
                        .Select(u => new { u.Gold, u.HeirloomPoints })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (buyer != null)
                    {
                        int buyerId = req.BuyerId.Value;
                        IQueryable<string> buyerTradableItemIds = _db.UserItems
                            .AsNoTracking()
                            .Where(ui => ui.UserId == buyerId
                                && !ui.IsBroken
                                && ui.PersonalItem == null
                                && ui.ClanArmoryBorrowedItem == null)
                            .Select(ui => ui.ItemId)
                            .Distinct();

                        query = query.Where(o => o.SellerId != buyerId
                            && o.Assets.Any(a =>
                                a.Side == MarketplaceOfferAssetSide.Requested
                                && a.Gold <= buyer.Gold
                                && a.HeirloomPoints <= buyer.HeirloomPoints
                                && (a.ItemId == null
                                    || buyerTradableItemIds.Contains(a.ItemId))));
                    }
                }
            }

            int totalCount = await query.CountAsync(cancellationToken);
            var offers = await query
                .AsSplitQuery()
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.Seller)
                    .ThenInclude(s => s!.ClanMembership)
                        .ThenInclude(cm => cm!.Clan)
                .Include(o => o.Assets)
                    .ThenInclude(a => a.Item)
                .Include(o => o.Assets)
                    .ThenInclude(a => a.UserItem!.Item)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new(new MarketplaceOffersPageViewModel
            {
                Items = _mapper.Map<IList<MarketplaceOfferViewModel>>(offers),
                TotalCount = totalCount,
            });
        }

        private static IQueryable<MarketplaceOffer> ApplySideFilters(
            IQueryable<MarketplaceOffer> query,
            MarketplaceOfferAssetSide side,
            SideFilter filter)
        {
            string? itemId = string.IsNullOrWhiteSpace(filter.ItemId) ? null : filter.ItemId;
            IList<int>? itemRanks = filter.ItemRanks;
            bool hasItemRanks = itemRanks is { Count: > 0 };
            ItemType? itemType = filter.ItemType;
            var goldFilter = NormalizeCurrencyFilter(filter.Gold);
            var heirloomPointsFilter = NormalizeCurrencyFilter(filter.HeirloomPoints);

            // Each offer has exactly one asset per side, so two separate .Where() calls
            // on the same side are guaranteed to filter the same single asset.

            // Item filter is side-specific (Offered uses UserItem, Requested uses Item/ItemId).
            if (itemId != null || hasItemRanks || itemType.HasValue)
            {
                if (side == MarketplaceOfferAssetSide.Offered)
                {
                    query = query.Where(o => o.Assets.Any(a =>
                        a.Side == side
                        && (itemId == null || (a.UserItem != null && a.UserItem.ItemId == itemId))
                        && (!hasItemRanks || (a.UserItem != null && a.UserItem.Item != null && itemRanks!.Contains(a.UserItem.Item.Rank)))
                        && (!itemType.HasValue || (a.UserItem != null && a.UserItem.Item != null && a.UserItem.Item.Type == itemType.Value))));
                }
                else
                {
                    query = query.Where(o => o.Assets.Any(a =>
                        a.Side == side
                        && (itemId == null || a.ItemId == itemId)
                        && (!hasItemRanks || (a.Item != null && itemRanks!.Contains(a.Item.Rank)))
                        && (!itemType.HasValue || (a.Item != null && a.Item.Type == itemType.Value))));
                }
            }

            // Currency filter is the same for both sides.
            if (goldFilter.MatchNone || goldFilter.HasRange || heirloomPointsFilter.MatchNone || heirloomPointsFilter.HasRange)
            {
                query = query.Where(o => o.Assets.Any(a =>
                    a.Side == side
                    && (!goldFilter.MatchNone || a.Gold == 0)
                    && (!goldFilter.HasRange || (a.Gold > 0 && a.Gold >= goldFilter.Min && a.Gold <= goldFilter.Max))
                    && (!heirloomPointsFilter.MatchNone || a.HeirloomPoints == 0)
                    && (!heirloomPointsFilter.HasRange
                        || (a.HeirloomPoints > 0
                            && a.HeirloomPoints >= heirloomPointsFilter.Min
                            && a.HeirloomPoints <= heirloomPointsFilter.Max))));
            }

            return query;
        }

        private static CurrencyFilterBounds NormalizeCurrencyFilter(MarketplaceCurrencyFilter filter)
        {
            if (filter.Mode == MarketplaceCurrencyFilterMode.None)
            {
                return new CurrencyFilterBounds(MatchNone: true, HasRange: false, Min: 0, Max: 0);
            }

            if (filter.Mode != MarketplaceCurrencyFilterMode.CustomRange || filter.Range is not { } range)
            {
                return new CurrencyFilterBounds(MatchNone: false, HasRange: false, Min: 0, Max: int.MaxValue);
            }

            int? rangeMin = range.min;
            int? rangeMax = range.max;
            int min = Math.Max(0, Math.Min(rangeMin ?? 0, rangeMax ?? int.MaxValue));
            int max = Math.Max(0, Math.Max(rangeMin ?? 0, rangeMax ?? int.MaxValue));

            return new CurrencyFilterBounds(MatchNone: false, HasRange: true, Min: min, Max: max);
        }

        private readonly record struct CurrencyFilterBounds(bool MatchNone, bool HasRange, int Min, int Max);
    }
}

public enum MarketplaceCurrencyFilterMode
{
    Any,
    None,
    CustomRange,
}

public readonly record struct MarketplaceCurrencyFilter
{
    public MarketplaceCurrencyFilterMode Mode { get; init; }
    public (int? min, int? max)? Range { get; init; }
}

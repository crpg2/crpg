using System.Text.Json;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Application.Marketplace.Models;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Marketplace.Queries;

public record GetMarketplaceListingsHistoryQuery : IMediatorRequest<MarketplaceListingsHistoryPageViewModel>
{
    public int? BuyerId { get; init; }
    public int? SellerId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 15;

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetMarketplaceListingsHistoryQuery, MarketplaceListingsHistoryPageViewModel>
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<MarketplaceListingsHistoryPageViewModel>> Handle(GetMarketplaceListingsHistoryQuery req, CancellationToken cancellationToken)
        {
            int page = Math.Max(1, req.Page);
            int pageSize = Math.Clamp(req.PageSize, 1, 100);

            IQueryable<ActivityLog> query = _db.ActivityLogs
                .AsNoTracking()
                .AsSplitQuery()
                .Include(l => l.Metadata)
                .Include(l => l.User!)
                    .ThenInclude(u => u.ClanMembership!)
                        .ThenInclude(cm => cm.Clan)
                .Where(l => l.Type == ActivityLogType.MarketplaceListingAccepted);

            if (req.BuyerId.HasValue && req.SellerId.HasValue && req.BuyerId.Value == req.SellerId.Value)
            {
                string sellerIdStr = req.SellerId.Value.ToString();
                query = query.Where(l => l.UserId == req.BuyerId.Value
                    || l.Metadata.Any(m => m.Key == "sellerId" && m.Value == sellerIdStr));
            }
            else
            {
                if (req.BuyerId.HasValue)
                {
                    query = query.Where(l => l.UserId == req.BuyerId.Value);
                }

                if (req.SellerId.HasValue)
                {
                    string sellerIdStr = req.SellerId.Value.ToString();
                    query = query.Where(l => l.Metadata.Any(m => m.Key == "sellerId" && m.Value == sellerIdStr));
                }
            }

            int totalCount = await query.CountAsync(cancellationToken);
            var logs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var sellerIds = logs
                .Select(l => l.Metadata.FirstOrDefault(m => m.Key == "sellerId")?.Value)
                .Where(v => int.TryParse(v, out _))
                .Select(v => int.Parse(v!))
                .Distinct()
                .ToList();

            var sellersById = await _db.Users
                .AsNoTracking()
                .AsSplitQuery()
                .Include(u => u.ClanMembership!)
                    .ThenInclude(cm => cm.Clan)
                .Where(u => sellerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, cancellationToken);

            var items = logs.Select(l => MapToHistoryViewModel(l, sellersById)).ToList();

            return new(new MarketplaceListingsHistoryPageViewModel
            {
                Items = items,
                TotalCount = totalCount,
            });
        }

        private MarketplaceListingHistoryViewModel MapToHistoryViewModel(ActivityLog log, Dictionary<int, User> sellersById)
        {
            var meta = log.Metadata.ToDictionary(m => m.Key, m => m.Value);

            _ = int.TryParse(meta.GetValueOrDefault("sellerId"), out int sellerId);
            _ = int.TryParse(meta.GetValueOrDefault("goldFee"), out int goldFee);
            _ = int.TryParse(meta.GetValueOrDefault("listingId"), out int listingId);

            sellersById.TryGetValue(sellerId, out var seller);

            return new MarketplaceListingHistoryViewModel
            {
                Id = listingId,
                Buyer = _mapper.Map<UserPublicViewModel>(log.User),
                Seller = _mapper.Map<UserPublicViewModel>(seller),
                GoldFee = goldFee,
                AcceptedAt = log.CreatedAt,
                Offer = DeserializeSide(meta.GetValueOrDefault("offer"), MarketplaceListingAssetSide.Offered),
                Request = DeserializeSide(meta.GetValueOrDefault("request"), MarketplaceListingAssetSide.Requested),
            };
        }

        private static MarketplaceListingAssetViewModel DeserializeSide(string? json, MarketplaceListingAssetSide side)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new() { Side = side };
            }

            try
            {
                var data = JsonSerializer.Deserialize<SideMetadata>(json, JsonOptions);
                return new MarketplaceListingAssetViewModel
                {
                    Side = side,
                    Gold = data?.Gold ?? 0,
                    HeirloomPoints = data?.HeirloomPoints ?? 0,
                    Item = data?.Item == null ? null : new ItemSummaryViewModel
                    {
                        Id = data.Item.Id,
                        BaseId = data.Item.BaseId,
                        Rank = data.Item.Rank,
                        Name = data.Item.Name,
                    },
                };
            }
            catch (JsonException)
            {
                return new() { Side = side };
            }
        }

        private record SideMetadata
        {
            public int Gold { get; init; }
            public int HeirloomPoints { get; init; }
            public ItemMetadata? Item { get; init; }
        }

        private record ItemMetadata
        {
            public string Id { get; init; } = string.Empty;
            public string BaseId { get; init; } = string.Empty;
            public int Rank { get; init; }
            public string Name { get; init; } = string.Empty;
        }
    }
}

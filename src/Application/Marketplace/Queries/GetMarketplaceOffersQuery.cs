using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Models;
using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Marketplace.Queries;

public record GetMarketplaceOffersQuery : IMediatorRequest<IList<MarketplaceOfferViewModel>>
{
    public string? RequestedItemId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetMarketplaceOffersQuery, IList<MarketplaceOfferViewModel>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<IList<MarketplaceOfferViewModel>>> Handle(GetMarketplaceOffersQuery req, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;

            var query = _db.MarketplaceOffers
                .AsNoTracking()
                .Include(o => o.Assets)
                    .ThenInclude(a => a.UserItem)
                        .ThenInclude(ui => ui!.Item)
                .Where(o => o.Status == MarketplaceOfferStatus.Active && o.ExpiresAt > now);

            if (!string.IsNullOrWhiteSpace(req.RequestedItemId))
            {
                query = query.Where(o => o.Assets.Any(a => a.Side == MarketplaceOfferAssetSide.Requested
                    && a.Type == MarketplaceAssetType.Item
                    && a.ItemId == req.RequestedItemId));
            }

            var offers = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);

            return new(_mapper.Map<IList<MarketplaceOfferViewModel>>(offers));
        }
    }
}

using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Marketplace.Queries;

public record GetSelfMarketplaceOffersQuery : IMediatorRequest<IList<MarketplaceOfferViewModel>>
{
    [JsonIgnore]
    public int UserId { get; set; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetSelfMarketplaceOffersQuery, IList<MarketplaceOfferViewModel>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<IList<MarketplaceOfferViewModel>>> Handle(GetSelfMarketplaceOffersQuery req, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.UtcNow;
            await MarketplaceOfferLifecycle.ExpireActiveOffersForUserAsync(_db, req.UserId, now, cancellationToken);

            var offers = await _db.MarketplaceOffers
                .AsNoTracking()
                .Include(o => o.Assets)
                    .ThenInclude(a => a.UserItem)
                        .ThenInclude(ui => ui!.Item)
                .Where(o => o.SellerUserId == req.UserId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);

            return new(_mapper.Map<IList<MarketplaceOfferViewModel>>(offers));
        }
    }
}

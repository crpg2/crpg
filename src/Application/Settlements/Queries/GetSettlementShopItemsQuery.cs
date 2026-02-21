using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Settlements.Queries;

public record GetSettlementShopItemsQuery : IMediatorRequest<IList<ItemViewModel>>
{
    public int PartyId { get; init; }
    public int SettlementId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap) : IMediatorRequestHandler<GetSettlementShopItemsQuery, IList<ItemViewModel>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IStrategusMap _strategusMap = strategusMap;

        public async ValueTask<Result<IList<ItemViewModel>>> Handle(GetSettlementShopItemsQuery req,
            CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            if ((party.Status != PartyStatus.IdleInSettlement
                 && party.Status != PartyStatus.RecruitingInSettlement)
                || party.CurrentSettlementId != req.SettlementId)
            {
                return new(CommonErrors.PartyNotInASettlement(party.Id));
            }

            var settlement = await _db.Settlements
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == req.SettlementId, cancellationToken);
            if (settlement == null)
            {
                return new(CommonErrors.SettlementNotFound(req.PartyId));
            }

            // Return items with the same culture as the settlement.
            var items = await _db.Items
                .AsNoTracking()
                .Where(i => i.Culture == Culture.Neutral || i.Culture == settlement.Culture)
                .ToArrayAsync(cancellationToken);
            return new(_mapper.Map<ItemViewModel[]>(items));
        }
    }
}

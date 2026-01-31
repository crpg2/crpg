using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Models;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Parties.Queries;

public record GetStrategusUpdateQuery : IMediatorRequest<StrategusUpdate>
{
    public int PartyId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap) : IMediatorRequestHandler<GetStrategusUpdateQuery, StrategusUpdate>
    {
        private static readonly PartyStatus[] VisibleStatuses =
        [
            PartyStatus.Idle,
            PartyStatus.AwaitingBattleJoinDecision,
            PartyStatus.AwaitingPartyOfferDecision,
        ];

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IStrategusMap _strategusMap = strategusMap;

        public async Task<Result<StrategusUpdate>> Handle(GetStrategusUpdateQuery req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                .AsSplitQuery()
                .Include(p => p.User!)
                    .ThenInclude(u => u.ClanMembership!.Clan)
                .Include(p => p.CurrentParty!.User)
                .Include(p => p.CurrentSettlement)
                .Include(p => p.CurrentBattle)
                .Include(p => p.Orders)
                    .ThenInclude(o => o.TargetedBattle)
                .Include(p => p.Orders)
                    .ThenInclude(o => o.TargetedSettlement)
                .Include(p => p.Orders)
                    .ThenInclude(o => o.TargetedParty)
                .FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var visibleParties = await _db.Parties
                .Where(h => h.Id != party.Id
                            && h.Position.IsWithinDistance(party.Position, _strategusMap.ViewDistance)
                            && VisibleStatuses.Contains(h.Status))
                .ProjectTo<PartyVisibleViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            var visibleSettlements = await _db.Settlements
                .Where(s => s.Position.IsWithinDistance(party.Position, _strategusMap.ViewDistance))
                .ProjectTo<SettlementPublicViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            var visibleBattles = await _db.Battles
                .Where(b => b.Position.IsWithinDistance(party.Position, _strategusMap.ViewDistance)
                            && b.Phase != BattlePhase.End)
                .ProjectTo<BattleViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            return new(new StrategusUpdate
            {
                Party = _mapper.Map<PartyViewModel>(party),
                VisibleParties = visibleParties,
                VisibleSettlements = visibleSettlements,
                VisibleBattles = visibleBattles,
            });
        }
    }
}

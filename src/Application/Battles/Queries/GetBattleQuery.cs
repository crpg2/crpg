using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattleQuery : IMediatorRequest<BattleDetailedViewModel>
{
    public int BattleId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetBattleQuery, BattleDetailedViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<BattleDetailedViewModel>> Handle(GetBattleQuery req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles
                .AsSplitQuery()
                // TODO: FIXME: optimize
                .Include(b => b.Fighters)
                    .ThenInclude(f => f.Party!.User)
                        .ThenInclude(u => u!.ClanMembership)
                            .ThenInclude(c => c!.Clan)
                .Include(b => b.Fighters)
                    .ThenInclude(f => f.Settlement)
                        .ThenInclude(s => s!.Owner)
                            .ThenInclude(o => o!.User)
                                .ThenInclude(u => u!.ClanMembership)
                                    .ThenInclude(c => c!.Clan)
                .Where(b => b.Id == req.BattleId)
                .FirstOrDefaultAsync(cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            // Battles in preparation shouldn't be visible to anyone but only to parties in sight on the map.
            if (battle.Phase == BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
            }

            var attackerCommander = battle.Fighters.First(f => f.Side == BattleSide.Attacker && f.Commander);
            var defenderCommander = battle.Fighters.First(f => f.Side == BattleSide.Defender && f.Commander);
            var battleType = defenderCommander.Settlement != null ? BattleType.Siege : BattleType.Battle;

            // TODO: FIXME: copypasta from GetBattlesQuery
            var battleVm = new BattleDetailedViewModel
            {
                Id = battle.Id,
                Region = battle.Region,
                Position = battle.Position,
                Phase = battle.Phase,
                Type = battleType,
                Attacker = _mapper.Map<BattleFighterViewModel>(attackerCommander),
                AttackerTotalTroops = battle.Fighters
                     .Where(f => f.Side == BattleSide.Attacker)
                     .Sum(f => (int)Math.Floor(f.Party!.Troops)),
                Defender = _mapper.Map<BattleFighterViewModel>(defenderCommander),
                DefenderTotalTroops = battle.Fighters
                     .Where(f => f.Side == BattleSide.Defender)
                     .Sum(f => (int)Math.Floor(f.Party?.Troops ?? 0) + (f.Settlement?.Troops ?? 0)),
                CreatedAt = battle.CreatedAt,
                ScheduledFor = battle.ScheduledFor,
            };

            return new(battleVm);
        }
    }
}

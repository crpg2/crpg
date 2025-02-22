using AutoMapper;
using AutoMapper.QueryableExtensions;
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
                .Include(b => b.Fighters).ThenInclude(f => f.Party!.User).ThenInclude(u => u!.ClanMembership).ThenInclude(c => c!.Clan)
                .Include(b => b.Fighters).ThenInclude(f => f.Settlement).ThenInclude(s => s!.Owner).ThenInclude(o => o!.User).ThenInclude(u => u!.ClanMembership).ThenInclude(c => c!.Clan)
                .Where(b => b.Id == req.BattleId)
                .FirstOrDefaultAsync();

            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            var battleVm = new BattleDetailedViewModel
            {
                Id = battle.Id,
                Region = battle.Region,
                Position = battle.Position,
                Phase = battle.Phase,
                Attacker = _mapper.Map<BattleFighterViewModel>(
                    battle.Fighters.First(f => f.Side == BattleSide.Attacker && f.Commander)),
                AttackerTotalTroops = battle.Fighters
                    .Where(f => f.Side == BattleSide.Attacker)
                    .Sum(f => (int)Math.Floor(f.Party!.Troops)),
                Defender = _mapper.Map<BattleFighterViewModel>(
                    battle.Fighters.FirstOrDefault(f => f.Side == BattleSide.Defender && f.Commander)),
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

using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattlesQuery : IMediatorRequest<IList<BattleDetailedViewModel>>
{
    public Region Region { get; init; }
    public BattleType? Type { get; init; }
    public IList<BattlePhase> Phases { get; init; } = Array.Empty<BattlePhase>();

    public class Validator : AbstractValidator<GetBattlesQuery>
    {
        public Validator()
        {
            RuleFor(q => q.Region).IsInEnum();
            RuleFor(q => q.Phases).ForEach(p =>
            {
                p.IsInEnum().NotEqual(BattlePhase.Preparation);
            });
        }
    }

    internal class Handler : IMediatorRequestHandler<GetBattlesQuery, IList<BattleDetailedViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<BattleDetailedViewModel>>> Handle(GetBattlesQuery req, CancellationToken cancellationToken)
        {
            var battles = await _db.Battles
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
                .Where(b =>
                    b.Region == req.Region &&
                    req.Phases.Contains(b.Phase) &&
                    (req.Type == null || (req.Type == BattleType.Siege
                            ? b.Fighters.Any(f => f.Side == BattleSide.Defender && f.Commander && f.Settlement != null)
                            : b.Fighters.Any(f => f.Side == BattleSide.Defender && f.Commander && f.Settlement == null))))
                .OrderBy(b => b.ScheduledFor)
                .ToArrayAsync(cancellationToken);

            // TODO: FIXME: copypasta from GetBattleQuery
            var battlesVm = battles.Select(b =>
            {
                var attackerCommander = b.Fighters.First(f => f.Side == BattleSide.Attacker && f.Commander);
                var defenderCommander = b.Fighters.First(f => f.Side == BattleSide.Defender && f.Commander);
                var battleType = defenderCommander.Settlement != null ? BattleType.Siege : BattleType.Battle;

                return new BattleDetailedViewModel
                {
                    Id = b.Id,
                    Region = b.Region,
                    Position = b.Position,
                    Phase = b.Phase,
                    Type = battleType,
                    Attacker = _mapper.Map<BattleFighterViewModel>(attackerCommander),
                    AttackerTotalTroops = b.Fighters
                            .Where(f => f.Side == BattleSide.Attacker)
                            .Sum(f => (int)Math.Floor(f.Party!.Troops)),
                    Defender = _mapper.Map<BattleFighterViewModel>(defenderCommander),
                    DefenderTotalTroops = b.Fighters
                            .Where(f => f.Side == BattleSide.Defender)
                            .Sum(f => (int)Math.Floor(f.Party?.Troops ?? 0) + (f.Settlement?.Troops ?? 0)),
                    CreatedAt = b.CreatedAt,
                    ScheduledFor = b.ScheduledFor,
                };
            }).ToArray();

            return new(battlesVm);
        }
    }
}

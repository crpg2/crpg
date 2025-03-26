using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record ScheduleBattleCommand : IMediatorRequest<BattleViewModel>
{
    public int PartyId { get; init; }
    public int BattleId { get; init; }
    public int Hour { get; init; }
    internal class Handler : IMediatorRequestHandler<ScheduleBattleCommand, BattleViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<ScheduleBattleCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IBattleScheduler _battleScheduler;

        public Handler(ICrpgDbContext db, IMapper mapper, IBattleScheduler battleScheduler)
        {
            _db = db;
            _mapper = mapper;
            _battleScheduler = battleScheduler;
        }

        public async Task<Result<BattleViewModel>> Handle(ScheduleBattleCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                .FirstOrDefaultAsync(p => p.Id == req.PartyId, cancellationToken);

            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var battle = await _db.Battles.Include(b => b.Fighters)
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);

            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            var attackerCommander = battle.Fighters.FirstOrDefault(f => f.Commander && f.Side == BattleSide.Attacker);

            if (attackerCommander?.Party?.Id != req.PartyId)
            {
                return new(CommonErrors.FighterNotACommander(req.PartyId, req.BattleId));
            }

            if (battle.Phase != BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(battle.Id, battle.Phase));
            }

            var defender = battle.Fighters.First(f => f.Commander && f.Side == BattleSide.Defender);
            if (defender.Party != null)
            {
                if (!defender.Party.VulnerabilityWindow.Get(battle.Region).Hours.Contains(req.Hour))
                {
                    return new(CommonErrors.PartyNotVulnerable(defender.Party.Id, req.Hour));
                }
            }
            else if (defender.Settlement?.Owner != null)
            {
                if (!defender.Settlement.Owner.VulnerabilityWindow.Get(battle.Region).Hours.Contains(req.Hour))
                {
                    return new(CommonErrors.PartyNotVulnerable(defender.Settlement.Owner.Id, req.Hour));
                }
            }

            battle.ScheduledFor = _battleScheduler.GetNextBattleDateFromHour(req.Hour);

            Logger.LogInformation("Battle '{0}' scheduled for '{1}' by '{2}'",
                battle.Id, battle.ScheduledFor, req.PartyId);

            await _db.SaveChangesAsync(cancellationToken);
            return new(_mapper.Map<BattleViewModel>(battle));
        }
    }
}

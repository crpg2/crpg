using Crpg.Application.Common;
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

public record UpdateBattlePhasesCommand : IMediatorRequest
{
    public TimeSpan DeltaTime { get; init; }

    internal class Handler(
        ICrpgDbContext db,
        IBattleParticipantDistributionModel battleParticipantDistributionModel,
        IBattleScheduler battleScheduler,
        IDateTime dateTime,
        Constants constants) : IMediatorRequestHandler<UpdateBattlePhasesCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateBattlePhasesCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IBattleParticipantDistributionModel _battleParticipantDistributionModel = battleParticipantDistributionModel;
        private readonly IBattleScheduler _battleScheduler = battleScheduler;
        private readonly IDateTime _dateTime = dateTime;
        private readonly TimeSpan _battleInitiationDuration = TimeSpan.FromHours(constants.StrategusBattleInitiationDurationHours);
        private readonly TimeSpan _battleHiringDuration = TimeSpan.FromHours(constants.StrategusBattleHiringDurationHours);

        public async ValueTask<Result> Handle(UpdateBattlePhasesCommand req, CancellationToken cancellationToken)
        {
            var battles = _db.Battles
                .AsSplitQuery()
                .Include(b => b.Fighters).ThenInclude(f => f.Party)
                .Include(b => b.Fighters).ThenInclude(f => f.Settlement)
                .Where(b =>
                    (b.Phase == BattlePhase.Preparation && b.CreatedAt + _battleInitiationDuration < _dateTime.UtcNow)
                    //
                    //
                    //
                    || (b.Phase == BattlePhase.Hiring && b.CreatedAt + _battleInitiationDuration + _battleHiringDuration < _dateTime.UtcNow)
                    || (b.Phase == BattlePhase.Scheduled && b.ScheduledFor < _dateTime.UtcNow))
                .AsAsyncEnumerable();

            await foreach (var battle in battles.WithCancellation(cancellationToken))
            {
                BattlePhase oldPhase = battle.Phase;
                switch (battle.Phase)
                {
                    case BattlePhase.Preparation:
                        int battleSlots = 100; // TODO: make it depend on the number of troops.
                        _battleParticipantDistributionModel.DistributeParticipants(battle.Fighters, battleSlots);
                        var fighterApplications = battle.FighterApplications.Where(ma => ma.Status == BattleFighterApplicationStatus.Pending).ToArray();
                        foreach (BattleFighterApplication application in fighterApplications)
                        {
                            application.Status = BattleFighterApplicationStatus.Declined;
                        }

                        // TODO: The start time, at least approximate, should be available at the Hiring stage.
                        battle.Phase = BattlePhase.Hiring;
                        break;
                    case BattlePhase.Hiring:
                        await _battleScheduler.ScheduleBattle(battle);

                        var applications = battle.MercenaryApplications.Where(ma => ma.Status == BattleMercenaryApplicationStatus.Pending).ToArray();
                        foreach (BattleMercenaryApplication application in applications)
                        {
                            application.Status = BattleMercenaryApplicationStatus.Declined;
                        }

                        battle.Phase = BattlePhase.Scheduled;
                        break;
                    case BattlePhase.Scheduled:
                        // TODO: startup game server...
                        battle.Phase = BattlePhase.Live;
                        break;
                }

                Logger.LogInformation("Battle '{0}' switches from phase '{1}' to phase '{2}'",
                    battle.Id, oldPhase, battle.Phase);
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }
    }
}

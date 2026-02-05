using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record RemoveBattleFighterCommand : IMediatorRequest
{
    [JsonIgnore]
    public int PartyId { get; init; }

    [JsonIgnore]
    public int BattleId { get; init; }

    [JsonIgnore]
    public int RemovedFighterId { get; init; }

    internal class Handler(ICrpgDbContext db, IActivityLogService activityLogService, IUserNotificationService userNotificationService) : IMediatorRequestHandler<RemoveBattleFighterCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RemoveBattleFighterCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IUserNotificationService _userNotificationService = userNotificationService;

        public async Task<Result> Handle(RemoveBattleFighterCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties.FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var battle = await _db.Battles.FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            if (battle.Phase != BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(battle.Id, battle.Phase));
            }

            var fighter = await _db.BattleFighters
                .Include(f => f.Party)
                .FirstOrDefaultAsync(p => p.BattleId == battle.Id && p.Id == req.RemovedFighterId, cancellationToken);
            if (fighter == null)
            {
                return new(CommonErrors.FighterNotFound(req.RemovedFighterId, battle.Id));
            }

            // TODO: проверка стороны

            /*
                 TODO: FIXME:
                attack cancellation mechanics
                defender retreat mechanics?
            */
            // if (req.PartyId == fighter.PartyId && fighter.Commander == true && fighter.Side == BattleSide.Attacker)
            // {

            // }

            if (req.PartyId == fighter.PartyId && fighter.Commander == false) // Fighter (not commander) is leaving the battle
            {
                _db.BattleFighters.Remove(fighter);
                fighter.Party!.Status = Domain.Entities.Parties.PartyStatus.Idle;
                fighter.Party!.CurrentBattleId = null;
                // TODO: add notification to commander
                // _db.ActivityLogs.Add(_activityLogService.CreateBattleParticipantLeavedLog(battle.Id, req.PartyId));
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}', battle fighter '{1}' left the battle'{2}'", req.PartyId, req.RemovedFighterId, req.BattleId);
                return new Result();
            }

            // Commander kick fighter from battle
            var battleFighter = await _db.BattleFighters.FirstOrDefaultAsync(f => f.BattleId == battle.Id && f.PartyId == req.PartyId, cancellationToken);
            if (battleFighter == null)
            {
                return new(CommonErrors.FighterNotFound(req.PartyId, req.BattleId));
            }

            if (battleFighter.Commander == false)
            {
                return new(CommonErrors.FighterNotACommander(battleFighter.Id, req.BattleId));
            }

            if (battleFighter.Side != fighter.Side)
            {
                // FIXME: TODO: checks for settlement
                return new(CommonErrors.PartiesNotOnTheSameSide((int)battleFighter.PartyId!, (int)fighter.PartyId!, req.BattleId));
            }

            _db.BattleFighters.Remove(fighter);

            // TODO: add notification to ex-battle figter
            // _db.ActivityLogs.Add(_activityLogService.CreateBattleParticipantKickedLog(battle.Id, participant.Character!.UserId, req.PartyId));
            // _db.UserNotifications.Add(_userNotificationService.CreateBattleParticipantKickedToExParticipantNotification(participant.Character!.UserId, battle.Id));
            fighter.Party!.Status = Domain.Entities.Parties.PartyStatus.Idle;
            fighter.Party!.CurrentBattleId = null;
            await _db.SaveChangesAsync(cancellationToken);
            // Logger.LogInformation("User '{0}' removed participant '{1}' (user '{2}') out of battle '{3}'", req.PartyId, req.RemovedParticipantId, participant.Character!.UserId, req.BattleId);
            return new Result();
        }
    }
}

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

public record RemoveBattleParticipantCommand : IMediatorRequest
{
    [JsonIgnore]
    public int PartyId { get; init; }

    [JsonIgnore]
    public int BattleId { get; init; }

    [JsonIgnore]
    public int RemovedParticipantId { get; init; }

    internal class Handler(ICrpgDbContext db, IActivityLogService activityLogService, IUserNotificationService userNotificationService) : IMediatorRequestHandler<RemoveBattleParticipantCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RemoveBattleParticipantCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IUserNotificationService _userNotificationService = userNotificationService;

        public async ValueTask<Result> Handle(RemoveBattleParticipantCommand req, CancellationToken cancellationToken)
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

            if (battle.Phase != BattlePhase.Hiring)
            {
                return new(CommonErrors.BattleInvalidPhase(battle.Id, battle.Phase));
            }

            var participant = await _db.BattleParticipants
                .Include(p => p.Character)
                .FirstOrDefaultAsync(p => p.BattleId == battle.Id && p.Id == req.RemovedParticipantId, cancellationToken);
            if (participant == null)
            {
                return new(CommonErrors.BattleParticipantNotFound(req.RemovedParticipantId, battle.Id));
            }

            if (participant.Type == BattleParticipantType.Party)
            {
                return new(CommonErrors.PartyFighter(req.PartyId, battle.Id));
            }

            if (req.PartyId == participant.Character!.UserId) // Participant is leaving the battle
            {
                _db.BattleParticipants.Remove(participant);
                _db.ActivityLogs.Add(_activityLogService.CreateBattleParticipantLeavedLog(battle.Id, req.PartyId));
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}', battle participant '{1}' left the battle '{1}'", req.PartyId, req.RemovedParticipantId, req.BattleId);
                return new Result();
            }

            var battleFighter = await _db.BattleFighters.FirstOrDefaultAsync(f => f.BattleId == battle.Id && f.PartyId == req.PartyId, cancellationToken);
            if (battleFighter == null)
            {
                return new(CommonErrors.FighterNotFound(req.PartyId, req.BattleId));
            }

            // TODO: FIXME: ADD TEST CASE
            if (battleFighter.Side != participant.Side)
            {
                return new(CommonErrors.PartiesNotOnTheSameSide(req.PartyId, participant.Character.UserId, req.BattleId));
            }

            _db.BattleParticipants.Remove(participant);
            _db.ActivityLogs.Add(_activityLogService.CreateBattleParticipantKickedLog(battle.Id, participant.Character!.UserId, req.PartyId));
            _db.UserNotifications.Add(_userNotificationService.CreateBattleParticipantKickedToExParticipantNotification(participant.Character!.UserId, battle.Id));
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' removed battle participant '{1}' (user '{2}') out of battle '{3}'", req.PartyId, req.RemovedParticipantId, participant.Character!.UserId, req.BattleId);
            return new Result();
        }
    }
}

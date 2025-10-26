using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
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

    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<RemoveBattleParticipantCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RemoveBattleParticipantCommand>();

        private readonly ICrpgDbContext _db = db;

        public async Task<Result> Handle(RemoveBattleParticipantCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties.FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var battle = await _db.Battles
                .AsSplitQuery()
                .Include(b => b.Participants.Where(m => m.Id == req.RemovedParticipantId))
                    .ThenInclude(m => m.Character)
                .Include(b => b.Fighters.Where(f => f.PartyId == req.PartyId))
            .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            if (battle.Phase != BattlePhase.Hiring)
            {
                return new(CommonErrors.BattleInvalidPhase(battle.Id, battle.Phase));
            }

            var participant = battle.Participants.FirstOrDefault();
            if (participant == null)
            {
                return new(CommonErrors.BattleParticipantNotFound(req.RemovedParticipantId));
            }

            if (participant.Type == BattleParticipantType.Party)
            {
                return new(CommonErrors.PartyFighter(req.PartyId, battle.Id));
            }

            if (req.PartyId == participant.Character!.UserId) // Participant is leaving the battle
            {
                _db.BattleParticipants.Remove(participant);
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("Participant '{0}' left the battle clan '{1}'", req.PartyId, req.BattleId);
                return new Result();
            }

            var battleFighter = battle.Fighters.FirstOrDefault();
            if (battleFighter == null)
            {
                return new(CommonErrors.FighterNotFound(req.PartyId, req.BattleId));
            }

            _db.BattleParticipants.Remove(participant);
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' removed participant '{1}' out of battle '{2}'", req.PartyId, req.RemovedParticipantId, req.BattleId);
            return new Result();
        }
    }
}

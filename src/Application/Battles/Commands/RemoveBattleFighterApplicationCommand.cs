using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record RemoveBattleFighterApplicationCommand : IMediatorRequest
{
    [JsonIgnore]
    public int PartyId { get; init; }
    [JsonIgnore]
    public int BattleId { get; init; }
    public BattleSide Side { get; init; }

    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<RemoveBattleFighterApplicationCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RemoveBattleFighterApplicationCommand>();

        private readonly ICrpgDbContext _db = db;

        public async ValueTask<Result> Handle(RemoveBattleFighterApplicationCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties.FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var battle = await _db.Battles
                .AsSplitQuery()
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            if (battle.Phase != BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(battle.Id, battle.Phase));
            }

            var fighterApplications = await _db.BattleFighterApplications
                .AsSplitQuery()
                .Where(a => a.BattleId == req.BattleId && a.PartyId == req.BattleId)
                .Include(a => a.Party)
                .ToArrayAsync(cancellationToken);

            // Only applications with the status Pending can be deleted
            var application = fighterApplications.FirstOrDefault(a => a.Status == BattleFighterApplicationStatus.Pending && a.Side == req.Side);
            if (application == null)
            {
                return new(CommonErrors.PendingBattleFighterApplicationNotExist(req.PartyId, req.BattleId, req.Side));
            }

            _db.BattleFighterApplications.Remove(application);

            bool hasOtherPendingApplications = battle.FighterApplications
                  .Any(a => a.Id != application.Id && a.Status == BattleFighterApplicationStatus.Pending);

            if (!hasOtherPendingApplications)
            {
                // TODO: to party service - LeaveFromBattle
                application.Party!.Status = PartyStatus.Idle;
                application.Party!.CurrentBattleId = null;
            }

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' removed their battle fighter application '{1}' out of battle '{2}' for the side '{3}'", req.PartyId,
                application.Id, req.BattleId, req.Side);

            return new Result();
        }
    }
}

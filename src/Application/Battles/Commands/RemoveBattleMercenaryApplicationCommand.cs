using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record RemoveBattleMercenaryApplicationCommand : IMediatorRequest
{
    [JsonIgnore]
    public int PartyId { get; init; }
    [JsonIgnore]
    public int BattleId { get; init; }
    public BattleSide Side { get; init; }

    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<RemoveBattleMercenaryApplicationCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RemoveBattleMercenaryApplicationCommand>();

        private readonly ICrpgDbContext _db = db;

        public async ValueTask<Result> Handle(RemoveBattleMercenaryApplicationCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties.FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var battle = await _db.Battles
                .AsSplitQuery()
                .Include(b => b.MercenaryApplications)
                    .ThenInclude(a => a.Character)
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            if (battle.Phase != BattlePhase.Hiring)
            {
                return new(CommonErrors.BattleInvalidPhase(battle.Id, battle.Phase));
            }

            // Only applications with the status Pending can be deleted.
            var mercenaryApplication = battle.MercenaryApplications.FirstOrDefault(a => a.Status == BattleMercenaryApplicationStatus.Pending && a.Character!.UserId == req.PartyId && a.Side == req.Side);
            if (mercenaryApplication == null)
            {
                return new(CommonErrors.ApplicationNotFound(0));
            }

            _db.BattleMercenaryApplications.Remove(mercenaryApplication);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' removed their mercenary application '{1}' out of battle '{2}' for the side '{3}'", req.PartyId,
                mercenaryApplication.Id, req.BattleId, req.Side);

            return new Result();
        }
    }
}

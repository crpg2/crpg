using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record RemoveBattleMercenaryApplicationCommand : IMediatorRequest
{
    public int UserId { get; init; }
    public int BattleId { get; init; }

    internal class Handler : IMediatorRequestHandler<RemoveBattleMercenaryApplicationCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RemoveBattleMercenaryApplicationCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IBattleService _battleService;

        public Handler(ICrpgDbContext db, IBattleService battleService)
        {
            _db = db;
            _battleService = battleService;
        }

        public async Task<Result> Handle(RemoveBattleMercenaryApplicationCommand req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles
                .AsSplitQuery()
                .Include(b => b.Fighters)
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            if (battle.Phase != BattlePhase.Hiring)
            {
                return new(CommonErrors.BattleInvalidPhase(battle.Id, battle.Phase));
            }

            var mercenaryApplicationResponse = await _battleService.GetBattleMercenaryApplication(_db, req.UserId, req.BattleId, cancellationToken);

            if (mercenaryApplicationResponse.Errors != null)
            {
                return new Result(mercenaryApplicationResponse.Errors);
            }

            _db.BattleMercenaryApplications.Remove(mercenaryApplicationResponse.Data!);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' removed their mercenary application '{1}' out of battle '{2}'", req.UserId,
                mercenaryApplicationResponse.Data!.Id, req.BattleId);
            return new Result();
        }
    }
}

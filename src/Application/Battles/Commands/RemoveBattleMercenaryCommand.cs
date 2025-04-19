using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record RemoveBattleMercenaryCommand : IMediatorRequest
{
    public int UserId { get; init; }
    public int BattleId { get; init; }
    public int RemovedMercenaryId { get; init; }

    internal class Handler : IMediatorRequestHandler<RemoveBattleMercenaryCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RemoveBattleMercenaryCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IBattleService _battleService;

        public Handler(ICrpgDbContext db, IBattleService battleService)
        {
            _db = db;
            _battleService = battleService;
        }

        public async Task<Result> Handle(RemoveBattleMercenaryCommand req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles
                .AsSplitQuery()
                .Include(b => b.Fighters)
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            var mercenary = await _db.BattleMercenaries
                .FirstOrDefaultAsync(bm => bm.Id == req.RemovedMercenaryId, cancellationToken);
            if (mercenary == null)
            {
                return new(CommonErrors.MercenaryNotFound(req.RemovedMercenaryId));
            }

            var fighterRes = await _battleService.GetBattleFighter(_db, req.UserId, req.BattleId, cancellationToken);
            var mercenaryRes = await _battleService.GetBattleMercenary(_db, req.UserId, req.BattleId, cancellationToken);

            if (fighterRes.Errors != null && mercenaryRes?.Data?.Id != mercenary.Id)
            {
                return new Result(fighterRes.Errors);
            }

            _db.BattleMercenaries.Remove(mercenary);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' removed mercenary '{1}' out of battle '{2}'", req.UserId,
                req.RemovedMercenaryId, req.BattleId);
            return new Result();
        }
    }
}

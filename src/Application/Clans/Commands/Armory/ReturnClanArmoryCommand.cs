using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands.Armory;

public record ReturnClanArmoryCommand : IMediatorRequest
{
    public int UserItemId { get; init; }
    public int UserId { get; init; }
    public int ClanId { get; init; }

    internal class Handler : IMediatorRequestHandler<ReturnClanArmoryCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<ReturnClanArmoryCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IActivityLogService _activityLogService;
        private readonly IClanService _clanService;

        public Handler(ICrpgDbContext db, IActivityLogService activityLogService, IClanService clanService)
        {
            _activityLogService = activityLogService;
            _db = db;
            _clanService = clanService;
        }

        public async Task<Result> Handle(ReturnClanArmoryCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Where(e => e.Id == req.UserId)
                .Include(e => e.ClanMembership)
                .FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            var clan = await _db.Clans
                .Where(e => e.Id == req.ClanId)
                .FirstOrDefaultAsync(cancellationToken);
            if (clan == null)
            {
                return new(CommonErrors.ClanNotFound(req.ClanId));
            }

            var result = await _clanService.ReturnArmoryItem(_db, clan, user, req.UserItemId, cancellationToken);
            if (result.Errors != null)
            {
                return new(result.Errors);
            }

            _db.ActivityLogs.Add(_activityLogService.CreateReturnClanArmoryItem(clan.Id, user.Id, req.UserItemId));

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' returned item '{1}' to the armory '{2}'", req.UserId, req.UserItemId, req.ClanId);

            return Result.NoErrors;
        }
    }
}
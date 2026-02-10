using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands.Armory;

public record RemoveItemFromClanArmoryCommand : IMediatorRequest
{
    [JsonIgnore]
    public int UserId { get; init; }

    [JsonIgnore]
    public int ClanId { get; init; }

    public int UserItemId { get; init; }

    internal class Handler : IMediatorRequestHandler<RemoveItemFromClanArmoryCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RemoveItemFromClanArmoryCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IClanService _clanService;
        private readonly IActivityLogService _activityLogService;

        public Handler(ICrpgDbContext db, IClanService clanService, IActivityLogService activityLogService)
        {
            _db = db;
            _clanService = clanService;
            _activityLogService = activityLogService;
        }

        public async ValueTask<Result> Handle(RemoveItemFromClanArmoryCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Where(u => u.Id == req.UserId)
                .Include(u => u.ClanMembership)
                .FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            var clan = await _db.Clans
                .Where(c => c.Id == req.ClanId)
                .FirstOrDefaultAsync(cancellationToken);
            if (clan == null)
            {
                return new(CommonErrors.ClanNotFound(req.ClanId));
            }

            var result = await _clanService.RemoveArmoryItem(_db, clan, user, req.UserItemId, cancellationToken);
            if (result.Errors != null)
            {
                return new(result.Errors);
            }

            _db.ActivityLogs.Add(_activityLogService.CreateRemoveItemFromClanArmoryLog(user.Id, clan.Id, req.UserItemId));

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' removed item '{1}' from the armory '{2}'", req.UserId, req.UserItemId, req.ClanId);

            return Result.NoErrors;
        }
    }
}

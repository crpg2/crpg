using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands.Armory;

public record ReturnItemToClanArmoryCommand : IMediatorRequest
{
    [JsonIgnore]
    public int UserId { get; init; }

    [JsonIgnore]
    public int ClanId { get; init; }

    public int UserItemId { get; init; }

    internal class Handler : IMediatorRequestHandler<ReturnItemToClanArmoryCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<ReturnItemToClanArmoryCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IClanService _clanService;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserNotificationService _userNotificationService;

        public Handler(ICrpgDbContext db, IClanService clanService, IActivityLogService activityLogService, IUserNotificationService userNotificationService)
        {
            _db = db;
            _clanService = clanService;
            _activityLogService = activityLogService;
            _userNotificationService = userNotificationService;
        }

        public async ValueTask<Result> Handle(ReturnItemToClanArmoryCommand req, CancellationToken cancellationToken)
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

            var result = await _clanService.ReturnArmoryItem(_db, clan, user, req.UserItemId, cancellationToken);
            if (result.Errors != null)
            {
                return new(result.Errors);
            }

            _db.ActivityLogs.Add(_activityLogService.CreateReturnItemToClanArmoryLog(user.Id, clan.Id, req.UserItemId));
            _db.UserNotifications.Add(_userNotificationService.CreateClanArmoryRemoveItemToBorrowerNotification(result.Data!.BorrowerUserId, clan.Id, result.Data!.UserItem!.ItemId, result.Data!.UserItem.UserId));

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' returned item '{1}' to the armory '{2}'", req.UserId, req.UserItemId, req.ClanId);

            return Result.NoErrors;
        }
    }
}

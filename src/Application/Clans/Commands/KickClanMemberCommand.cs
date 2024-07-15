using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Users;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands;

public record KickClanMemberCommand : IMediatorRequest
{
    public int UserId { get; init; }
    public int ClanId { get; init; }
    public int KickedUserId { get; init; }

    internal class Handler : IMediatorRequestHandler<KickClanMemberCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<KickClanMemberCommand>();

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

        public async Task<Result> Handle(KickClanMemberCommand req, CancellationToken cancellationToken)
        {
            var userRes = await _clanService.GetClanMember(_db, req.UserId, req.ClanId, cancellationToken);
            if (userRes.Errors != null)
            {
                return new Result(userRes.Errors);
            }

            User user = userRes.Data!;
            if (req.UserId == req.KickedUserId) // User is leaving the clan
            {
                var leaveClanRes = await _clanService.LeaveClan(_db, user.ClanMembership!, cancellationToken);
                if (leaveClanRes.Errors != null)
                {
                    return leaveClanRes;
                }

                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}' left clan '{1}'", req.UserId, req.ClanId);
                return new Result();
            }

            var kickedUserRes = await _clanService.GetClanMember(_db, req.KickedUserId, req.ClanId, cancellationToken);
            if (kickedUserRes.Errors != null)
            {
                return new Result(kickedUserRes.Errors);
            }

            User kickedUser = kickedUserRes.Data!;
            if (user.ClanMembership!.Role < kickedUser.ClanMembership!.Role)
            {
                return new Result(CommonErrors.ClanMemberRoleNotMet(req.UserId, kickedUser.ClanMembership.Role + 1,
                    user.ClanMembership.Role));
            }

            _db.ClanMembers.Remove(kickedUser.ClanMembership);

            var clanMemberKickedActivityLog = _activityLogService.CreateClanMemberKickedLog(req.KickedUserId, req.ClanId, req.UserId);
            _db.ActivityLogs.Add(clanMemberKickedActivityLog);
            _db.UserNotifications.Add(_userNotificationService.CreateClanMemberKickedToExMemberNotification(req.KickedUserId, clanMemberKickedActivityLog.Id));

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' kicked user '{1}' out of clan '{2}'", req.UserId,
                req.KickedUserId, req.ClanId);
            return new Result();
        }
    }
}

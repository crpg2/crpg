using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Users.Commands;

public record RewardUserCommand : IMediatorRequest<UserViewModel>
{
    public int UserId { get; init; }
    public int ActorUserId { get; init; }
    public int Gold { get; init; }
    public int HeirloomPoints { get; init; }
    public string ItemId { get; init; } = string.Empty;

    internal class Handler : IMediatorRequestHandler<RewardUserCommand, UserViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RewardUserCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserNotificationService _userNotificationService;

        public Handler(ICrpgDbContext db, IMapper mapper, IActivityLogService activityLogService, IUserNotificationService userNotificationService)
        {
            _db = db;
            _mapper = mapper;
            _activityLogService = activityLogService;
            _userNotificationService = userNotificationService;
        }

        public async Task<Result<UserViewModel>> Handle(RewardUserCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            user.Gold = Math.Max(user.Gold + req.Gold, 0);
            user.HeirloomPoints = Math.Max(user.HeirloomPoints + req.HeirloomPoints, 0);

            if (req.ItemId != string.Empty)
            {
                var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
                if (item == null)
                {
                    return new(CommonErrors.ItemNotFound(req.ItemId));
                }

                var existingPersonalItems = await _db.PersonalItems
                    .Include(pi => pi.UserItem)
                        .ThenInclude(ui => ui!.Item)
                    .Where(pi => pi.UserItem!.UserId == req.UserId)
                    .ToDictionaryAsync(pi => pi.UserItem!.Item!.Id, cancellationToken);

                if (existingPersonalItems.ContainsKey(item.Id))
                {
                    return new(CommonErrors.PersonalItemAlreadyExist(req.UserId, req.ItemId));
                }

                user.Items.Add(new UserItem
                {
                    UserId = req.UserId,
                    Item = item,
                    PersonalItem = new(),
                });
            }

            var activityLog = _activityLogService.CreateUserRewardedLog(req.UserId, req.ActorUserId, req.Gold, req.HeirloomPoints, req.ItemId);
            _db.ActivityLogs.Add(activityLog);
            _db.UserNotifications.Add(_userNotificationService.CreateUserRewardedToUserNotification(req.UserId, activityLog.Id));

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' rewarded", req.UserId);

            return new Result<UserViewModel>(_mapper.Map<UserViewModel>(user));
        }
    }
}

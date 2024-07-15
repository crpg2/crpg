using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Notifications.Models;
using Crpg.Application.Users.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Notifications.Queries;

public record GetUserNotificationsQuery : IMediatorRequest<UserNotificationsWithDictViewModel>
{
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetUserNotificationsQuery, UserNotificationsWithDictViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IActivityLogService _activityLogService;

        public Handler(ICrpgDbContext db, IMapper mapper, IActivityLogService activityLogService)
        {
            _db = db;
            _mapper = mapper;
            _activityLogService = activityLogService;
        }

        // TODO: all + pagination/filters (by date? by type?)
        public async Task<Result<UserNotificationsWithDictViewModel>> Handle(GetUserNotificationsQuery req,
            CancellationToken cancellationToken)
        {
            var userNotifications = await _db.UserNotifications
                .Include(un => un.ActivityLog)
                    .ThenInclude(al => al!.Metadata)
                .Where(un => un.UserId == req.UserId)
                .OrderByDescending(un => un.CreatedAt)
                .Take(1000) // TODO:
                .ToArrayAsync(cancellationToken);

            var entitiesFromMetadata = _activityLogService.ExtractEntitiesFromMetadata(userNotifications.Select(un => un.ActivityLog!).ToArray());
            var clans = await _db.Clans.Where(c => entitiesFromMetadata.ClansIds.Contains(c.Id)).ToArrayAsync();
            var users = await _db.Users.Where(u => entitiesFromMetadata.UsersIds.Contains(u.Id)).ToArrayAsync();
            var characters = await _db.Characters.Where(c => entitiesFromMetadata.CharactersIds.Contains(c.Id)).ToArrayAsync();

            return new(new UserNotificationsWithDictViewModel()
            {
                Notifications = _mapper.Map<IList<UserNotificationViewModel>>(userNotifications),
                Dict = new()
                {
                    Clans = _mapper.Map<IList<ClanPublicViewModel>>(clans),
                    Users = _mapper.Map<IList<UserPublicViewModel>>(users),
                    Characters = _mapper.Map<IList<CharacterPublicViewModel>>(characters),
                },
            });
        }
    }
}

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
        private readonly IMetadataService _metadataService;

        public Handler(ICrpgDbContext db, IMapper mapper, IMetadataService metadataService)
        {
            _db = db;
            _mapper = mapper;
            _metadataService = metadataService;
        }

        // TODO: FIXME: all + pagination/filters (by date? by type?)
        public async ValueTask<Result<UserNotificationsWithDictViewModel>> Handle(GetUserNotificationsQuery req,
            CancellationToken cancellationToken)
        {
            var userNotifications = await _db.UserNotifications
                .Include(un => un.Metadata)
                .Where(un => un.UserId == req.UserId)
                .OrderByDescending(un => un.CreatedAt)
                .Take(1000) // TODO: FIXME:
                .ToArrayAsync(cancellationToken);

            var entitiesFromMetadata = _metadataService.ExtractEntitiesFromMetadata(userNotifications
                .SelectMany(al => al.Metadata)
                .Select(m => new KeyValuePair<string, string>(m.Key, m.Value)));
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

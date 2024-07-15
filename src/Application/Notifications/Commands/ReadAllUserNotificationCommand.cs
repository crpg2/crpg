using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Notifications.Commands;

public record ReadAllUserNotificationCommand : IMediatorRequest
{
    public int UserNotificationId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<ReadAllUserNotificationCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<ReadAllUserNotificationCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result> Handle(ReadAllUserNotificationCommand req, CancellationToken cancellationToken)
        {
            var userNotifications = await _db.UserNotifications
             .Where(un => un.UserId == req.UserId)
             .ToArrayAsync(cancellationToken);

            foreach (var userNotification in userNotifications)
            {
                userNotification.State = Domain.Entities.Notification.NotificationState.Read;
            }

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' updated the notification '{1}'", req.UserId, req.UserNotificationId);
            return new Result();
        }
    }
}

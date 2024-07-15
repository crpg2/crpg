using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Notifications.Commands;

public record DeleteUserNotificationCommand : IMediatorRequest
{
    public int UserNotificationId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<DeleteUserNotificationCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteUserNotificationCommand>();

        private readonly ICrpgDbContext _db;

        public Handler(ICrpgDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(DeleteUserNotificationCommand req, CancellationToken cancellationToken)
        {
            var userNotification = await _db.UserNotifications
               .FirstOrDefaultAsync(un => un.Id == req.UserNotificationId && un.UserId == req.UserId, cancellationToken);

            if (userNotification == null)
            {
                return new(CommonErrors.UserNotificationNotFound(req.UserId, req.UserNotificationId));
            }

            _db.UserNotifications.Remove(userNotification);

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' delete the notification '{1}'", req.UserId, req.UserNotificationId);
            return new Result();
    }
    }
}

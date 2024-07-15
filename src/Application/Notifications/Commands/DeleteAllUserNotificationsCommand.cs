using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Notifications.Commands;

public record DeleteAllUserNotificationsCommand : IMediatorRequest
{
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<DeleteAllUserNotificationsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteAllUserNotificationsCommand>();

        private readonly ICrpgDbContext _db;

        public Handler(ICrpgDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(DeleteAllUserNotificationsCommand req, CancellationToken cancellationToken)
        {
            var userNotifications = await _db.UserNotifications
               .Where(un => un.UserId == req.UserId)
               .ToArrayAsync(cancellationToken);

            _db.UserNotifications.RemoveRange(userNotifications);

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' delete all notifications", req.UserId);
            return new Result();
        }
    }
}

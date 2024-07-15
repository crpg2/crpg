using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Notifications.Commands;

public record DeleteOldUserNotificationsCommand : IMediatorRequest
{
    internal class Handler : IMediatorRequestHandler<DeleteOldUserNotificationsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteOldUserNotificationsCommand>();
        private static readonly TimeSpan LogRetention = TimeSpan.FromDays(30);

        private readonly ICrpgDbContext _db;
        private readonly IDateTime _dateTime;

        public Handler(ICrpgDbContext db, IDateTime dateTime)
        {
            _db = db;
            _dateTime = dateTime;
        }

        public async Task<Result> Handle(DeleteOldUserNotificationsCommand req, CancellationToken cancellationToken)
        {
            var limit = _dateTime.UtcNow - LogRetention;
            var userNotifications = await _db.UserNotifications
                .Where(l => l.CreatedAt < limit)
                .ToArrayAsync(cancellationToken);

            // ExecuteDelete can't be used because it is not supported by the in-memory provider which is used in our
            // tests (https://github.com/dotnet/efcore/issues/30185).
            _db.UserNotifications.RemoveRange(userNotifications);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("{0} old user notifications were cleaned out", userNotifications.Length);

            return Result.NoErrors;
        }
    }
}

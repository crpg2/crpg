using System.Collections.Frozen;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.ActivityLogs.Commands;

public record DeleteOldActivityLogsCommand : IMediatorRequest
{
    internal class Handler(ICrpgDbContext db, IDateTime dateTime) : IMediatorRequestHandler<DeleteOldActivityLogsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteOldActivityLogsCommand>();
        private static readonly TimeSpan DefaultLogRetention = TimeSpan.FromDays(30);
        private static readonly TimeSpan CandidateLogRetention = TimeSpan.FromDays(15);
        private static readonly FrozenDictionary<ActivityLogType, TimeSpan> LogRetentionByType = new Dictionary<ActivityLogType, TimeSpan>
        {
            [ActivityLogType.CharacterEarned] = TimeSpan.FromDays(15),
            [ActivityLogType.MarketplaceOfferCreated] = TimeSpan.FromDays(180),
            [ActivityLogType.MarketplaceOfferCancelled] = TimeSpan.FromDays(180),
            [ActivityLogType.MarketplaceOfferExpired] = TimeSpan.FromDays(180),
        }.ToFrozenDictionary();

        private readonly ICrpgDbContext _db = db;
        private readonly IDateTime _dateTime = dateTime;

        public async ValueTask<Result> Handle(DeleteOldActivityLogsCommand req, CancellationToken cancellationToken)
        {
            var now = _dateTime.UtcNow;
            var limit = now - CandidateLogRetention;
            var activityLogs = await _db.ActivityLogs
                .Where(l => l.CreatedAt < limit)
                .Include(l => l.Metadata)
                .ToArrayAsync(cancellationToken);

            var activityLogsToDelete = activityLogs
                .Where(l => l.CreatedAt < now - GetRetention(l.Type))
                .ToArray();

            // ExecuteDelete can't be used because it is not supported by the in-memory provider which is used in our
            // tests (https://github.com/dotnet/efcore/issues/30185).
            _db.ActivityLogs.RemoveRange(activityLogsToDelete);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("{0} old activity logs were cleaned out", activityLogsToDelete.Length);

            return Result.NoErrors;
        }

        private static TimeSpan GetRetention(ActivityLogType type)
        {
            return LogRetentionByType.TryGetValue(type, out TimeSpan retention)
                ? retention
                : DefaultLogRetention;
        }
    }
}

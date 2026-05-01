using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.GameEvents.Commands;

public record DeleteOldGameEventsCommand : IMediatorRequest
{
    internal class Handler(ICrpgDbContext db, IDateTime dateTime) : IMediatorRequestHandler<DeleteOldGameEventsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteOldGameEventsCommand>();
        private static readonly TimeSpan Retention = TimeSpan.FromDays(30);

        private readonly ICrpgDbContext _db = db;
        private readonly IDateTime _dateTime = dateTime;

        public async ValueTask<Result> Handle(DeleteOldGameEventsCommand req, CancellationToken cancellationToken)
        {
            var limit = _dateTime.UtcNow - Retention;
            var gameEvents = await _db.GameEvents
                .Where(e => e.CreatedAt < limit)
                .ToArrayAsync(cancellationToken);

            // ExecuteDelete can't be used because it is not supported by the in-memory provider which is used in our
            // tests (https://github.com/dotnet/efcore/issues/30185).
            _db.GameEvents.RemoveRange(gameEvents);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("{0} old game events were cleaned out", gameEvents.Length);
            return new Result();
        }
    }
}

using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.IdempotencyKeys.Commands;

public record DeleteOldIdempotencyKeysCommand : IMediatorRequest
{
    internal class Handler : IMediatorRequestHandler<DeleteOldIdempotencyKeysCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteOldIdempotencyKeysCommand>();
        private static readonly TimeSpan Retention = TimeSpan.FromDays(1);

        private readonly ICrpgDbContext _db;
        private readonly IDateTime _dateTime;

        public Handler(ICrpgDbContext db, IDateTime dateTime)
        {
            _db = db;
            _dateTime = dateTime;
        }

        public async ValueTask<Result> Handle(DeleteOldIdempotencyKeysCommand req, CancellationToken cancellationToken)
        {
            var limit = _dateTime.UtcNow - Retention;
            var idempotencyKeys = await _db.IdempotencyKeys
                .Where(l => l.CreatedAt < limit)
                .ToArrayAsync(cancellationToken);

            // ExecuteDelete can't be used because it is not supported by the in-memory provider which is used in our
            // tests (https://github.com/dotnet/efcore/issues/30185).
            _db.IdempotencyKeys.RemoveRange(idempotencyKeys);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("{0} old idempotency keys were cleaned out", idempotencyKeys.Length);

            return Result.NoErrors;
        }
    }
}

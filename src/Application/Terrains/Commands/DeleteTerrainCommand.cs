using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Terrains.Commands;

public record DeleteTerrainCommand : IMediatorRequest
{
    public int Id { get; init; }

    internal class Handler : IMediatorRequestHandler<DeleteTerrainCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteTerrainCommand>();

        private readonly ICrpgDbContext _db;

        public Handler(ICrpgDbContext db)
        {
            _db = db;
        }

        public async ValueTask<Result> Handle(DeleteTerrainCommand req, CancellationToken cancellationToken)
        {
            var terrain = await _db.Terrains
                .FirstOrDefaultAsync(t => t.Id == req.Id, cancellationToken);
            if (terrain == null)
            {
                return new Result(CommonErrors.TerrainNotFound(req.Id));
            }

            _db.Terrains.Remove(terrain);

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Terrain '{0}' has been deleted", req.Id);
            return Result.NoErrors;
        }
    }
}

using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Terrains.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Terrains.Commands;

public record UpdateTerrainCommand : IMediatorRequest<TerrainViewModel>
{
    public int Id { get; init; }
    public Polygon Boundary { get; set; } = default!;

    internal class Handler : IMediatorRequestHandler<UpdateTerrainCommand, TerrainViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateTerrainCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async ValueTask<Result<TerrainViewModel>> Handle(UpdateTerrainCommand req, CancellationToken cancellationToken)
        {
            var terrain = await _db.Terrains
                .FirstOrDefaultAsync(t => t.Id == req.Id, cancellationToken);
            if (terrain == null)
            {
                return new(CommonErrors.TerrainNotFound(req.Id));
            }

            terrain.Boundary = req.Boundary;

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Terrain '{0}' updated", req.Id);

            return new Result<TerrainViewModel>(_mapper.Map<TerrainViewModel>(terrain));
        }
    }
}

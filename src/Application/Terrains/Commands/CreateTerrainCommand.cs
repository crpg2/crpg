using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Terrains.Models;
using Crpg.Domain.Entities.Terrains;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Terrains.Commands;

public record CreateTerrainCommand : IMediatorRequest<TerrainViewModel>
{
    public TerrainType Type { get; set; }
    public Polygon Boundary { get; set; } = default!;

    internal class Handler : IMediatorRequestHandler<CreateTerrainCommand, TerrainViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreateTerrainCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async ValueTask<Result<TerrainViewModel>> Handle(CreateTerrainCommand req, CancellationToken cancellationToken)
        {
            Terrain terrain = new()
            {
                Type = req.Type,
                Boundary = req.Boundary,
            };

            _db.Terrains.Add(terrain);

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Terrain type '{0}' with boundary '{1}' add to Strategus map", req.Type, req.Boundary);
            return new(_mapper.Map<TerrainViewModel>(terrain));
        }
    }
}

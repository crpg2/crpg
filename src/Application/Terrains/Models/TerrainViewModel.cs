using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Terrains;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Terrains.Models;

public record TerrainViewModel : IMapFrom<Terrain>
{
    public int Id { get; init; }
    public TerrainType Type { get; init; }
    public Polygon Boundary { get; init; } = default!;
}

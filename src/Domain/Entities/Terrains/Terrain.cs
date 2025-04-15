using Crpg.Domain.Common;
using NetTopologySuite.Geometries;

namespace Crpg.Domain.Entities.Terrains;

/// <summary>
/// Represents a terrain (forest, mount, river, .etc) on Strategus.
/// </summary>
public class Terrain : AuditableEntity
{
    public int Id { get; set; }
    public TerrainType Type { get; set; }
    public Polygon Boundary { get; set; } = Polygon.Empty;
}

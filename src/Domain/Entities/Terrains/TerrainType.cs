namespace Crpg.Domain.Entities.Terrains;

/* TODO:
Roads?
- top tier
- low tier
- bridge

Snow?
Sand?

Plain - Default
*/

/// <summary>
/// Type of <see cref="Terrain"/>.
/// </summary>
public enum TerrainType
{
    /// <summary></summary>
    Barrier,

    /// <summary></summary>
    ThickForest,

    /// <summary></summary>
    SparseForest,

    /// <summary></summary>
    ShallowWater,

    /// <summary></summary>
    DeepWater,

    /// <summary></summary>
    Plain,
}

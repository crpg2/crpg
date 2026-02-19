using Crpg.Domain.Entities.Terrains;

namespace Crpg.Application.Parties.Models;

/// <summary>
/// Breakdown of party speed calculation components for UI display.
/// </summary>
public record PartySpeed
{
    /// <summary>Base speed before any modifiers.</summary>
    public double BaseSpeed { get; init; }

    /// <summary>Terrain speed factor (from Terrain Type).</summary>
    public double TerrainInfluence { get; init; }

    /// <summary>Current terrain type, if any.</summary>
    public TerrainType? CurrentTerrainType { get; init; }

    /// <summary>Weight factor (reserved for future use).</summary>
    public double WeightFactor { get; init; }

    /// <summary>Mount speed influence based on available mounts.</summary>
    public double MountInfluence { get; init; }

    /// <summary>Troop count influence on speed.</summary>
    public double TroopInfluence { get; init; }

    /// <summary>Base speed without terrain influence.</summary>
    public double BaseSpeedWithoutTerrain { get; init; }

    /// <summary>Final calculated speed.</summary>
    public double FinalSpeed { get; init; }
}

using NetTopologySuite.Geometries;

namespace Crpg.Application.Parties.Models;

public record PartyOrderPathSegmentViewModel
{
    public Point StartPoint { get; init; } = default!;
    public Point EndPoint { get; init; } = default!;
    public double Distance { get; init; }
    public double SpeedMultiplier { get; init; }
    public double Speed { get; init; }
}

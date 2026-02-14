using Crpg.Domain.Entities.Terrains;
using NetTopologySuite.Geometries;

namespace Crpg.Application.Common.Services;

public interface IStrategusRouting
{
    /// <summary>
    /// Builds path segments based on terrain boundary intersections.
    /// </summary>
    /// <param name="start">Starting point of the path.</param>
    /// <param name="end">Ending point of the path.</param>
    /// <param name="terrains">Array of terrain polygons.</param>
    /// <returns>List of path segments with terrain multipliers.</returns>
    List<PathSegment> BuildPathSegments(Point start, Point end, Terrain[] terrains);

    /// <summary>
    /// Interpolates a point along the line from start to end.
    /// </summary>
    /// <param name="start">Starting point.</param>
    /// <param name="end">Ending point.</param>
    /// <param name="ratio">Interpolation ratio (0.0 to 1.0).</param>
    /// <returns>Interpolated point.</returns>
    Point InterpolatePoint(Point start, Point end, double ratio);

    double GetTerrainSpeedMultiplier(TerrainType terrainType);

    double GetTerrainSpeedMultiplier(Point position, Terrain[] terrains);
}

/// <summary>
/// Represents a segment of a path with terrain information.
/// </summary>
public record PathSegment
{
    public required Point StartPoint { get; init; }
    public required Point EndPoint { get; init; }
    public required double TerrainMultiplier { get; init; }
}

internal class StrategusRouting() : IStrategusRouting
{
    private readonly Dictionary<TerrainType, double> terrainSpeedMultiplier = new()
    {
        { TerrainType.Plain, 1.0 },
        { TerrainType.SparseForest, 0.8 },
        { TerrainType.ThickForest, 0.5 },
        { TerrainType.Barrier, 0 },
        { TerrainType.DeepWater, 0 },
        { TerrainType.ShallowWater, 0.2 },
    };

    public double GetTerrainSpeedMultiplier(TerrainType terrainType)
    {
        return terrainSpeedMultiplier[terrainType];
    }

    public double GetTerrainSpeedMultiplier(Point position, Terrain[] terrains)
    {
        var terrain = terrains.FirstOrDefault(terrain => terrain.Boundary.Contains(position));
        if (terrain == null)
        {
            return 1.0;
        }

        return GetTerrainSpeedMultiplier(terrain.Type);
    }

    public List<PathSegment> BuildPathSegments(Point start, Point end, Terrain[] terrains)
    {
        var segments = new List<PathSegment>();
        SortedSet<double> intersectionPoints = [];
        double totalDistance = start.Distance(end);

        intersectionPoints.Add(0);
        intersectionPoints.Add(totalDistance);

        var pathLine = start.Factory.CreateLineString([start.Coordinate, end.Coordinate]);
        foreach (var terrain in terrains)
        {
            if (pathLine.Intersects(terrain.Boundary))
            {
                var intersection = pathLine.Intersection(terrain.Boundary);
                ExtractIntersectionPoints(intersection, start, intersectionPoints);
            }
        }

        var sortedDistances = intersectionPoints.ToList();
        for (int i = 0; i < sortedDistances.Count - 1; i++)
        {
            double startDist = sortedDistances[i];
            double endDist = sortedDistances[i + 1];

            Point segmentStart = startDist == 0 ? start : InterpolatePoint(start, end, startDist / totalDistance);
            Point segmentEnd = endDist == totalDistance ? end : InterpolatePoint(start, end, endDist / totalDistance);

            Point midPoint = InterpolatePoint(segmentStart, segmentEnd, 0.5);
            double terrainMultiplier = GetTerrainSpeedMultiplier(midPoint, terrains);

            segments.Add(new PathSegment
            {
                StartPoint = segmentStart,
                EndPoint = segmentEnd,
                TerrainMultiplier = terrainMultiplier,
            });
        }

        return segments;
    }

    public Point InterpolatePoint(Point start, Point end, double ratio)
    {
        double x = start.X + (end.X - start.X) * ratio;
        double y = start.Y + (end.Y - start.Y) * ratio;
        return start.Factory.CreatePoint(new Coordinate(x, y));
    }

    /// <summary>
    /// Extracts intersection points from a geometry and adds their distances to the set.
    /// </summary>
    private static void ExtractIntersectionPoints(Geometry intersection, Point reference, SortedSet<double> distances)
    {
        if (intersection is Point p)
        {
            distances.Add(reference.Distance(p));
        }
        else if (intersection is MultiPoint mp)
        {
            foreach (Point point in mp.Geometries.Cast<Point>())
            {
                distances.Add(reference.Distance(point));
            }
        }
        else if (intersection is LineString ls)
        {
            var coords = ls.Coordinates;
            if (coords.Length > 0)
            {
                distances.Add(reference.Distance(ls.Factory.CreatePoint(coords[0])));
                distances.Add(reference.Distance(ls.Factory.CreatePoint(coords[^1])));
            }
        }
        else if (intersection is GeometryCollection gc)
        {
            foreach (var geom in gc.Geometries)
            {
                ExtractIntersectionPoints(geom, reference, distances);
            }
        }
    }
}

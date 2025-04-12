using Crpg.Application.Common.Results;
using Crpg.Application.Terrains.Commands;
using Crpg.Domain.Entities.Terrains;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Terrains;

public class UpdateTerrainCommandTest : TestBase
{
    [Test]
    public async Task ShouldUpdateTerrain()
    {
        Terrain[] terrains =
            {
                new()
                {
                    Type = TerrainType.ThickForest,
                    Boundary = new Polygon(new LinearRing(new Coordinate[] { new(1.1, 1.2), new(2.1, 2.2) })),
                },
                new()
                {
                    Type = TerrainType.ShallowWater,
                    Boundary = new Polygon(new LinearRing(Array.Empty<Coordinate>())),
                },
            };

        ArrangeDb.Terrains.AddRange(terrains);
        await ArrangeDb.SaveChangesAsync();

        var result = await new UpdateTerrainCommand.Handler(ActDb, Mapper).Handle(
            new UpdateTerrainCommand
            {
                Id = 1,
                Boundary = new Polygon(new LinearRing(new Coordinate[] { new(5.1, 5.2), new(6.1, 6.2) })),
            }, CancellationToken.None);

        var terrain = result.Data!;
        Assert.That(terrain.Boundary, Is.EqualTo(new Polygon(new LinearRing(new Coordinate[] { new(5.1, 5.2), new(6.1, 6.2) }))));
    }

    [Test]
    public async Task NotFoundTerrain()
    {
        var result = await new UpdateTerrainCommand.Handler(ActDb, Mapper).Handle(
            new UpdateTerrainCommand
            {
                Id = 1,
                Boundary = new Polygon(new LinearRing(Array.Empty<Coordinate>())),
            }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.TerrainNotFound));
    }
}

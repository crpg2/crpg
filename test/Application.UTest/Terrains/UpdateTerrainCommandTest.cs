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
                    Boundary = new Polygon(new LinearRing(new Coordinate[] { new(104.174348, -97.761932), new(104.130833, -98.066596), new(103.420365, -98.192822), new(102.686134, -97.974021), new(101.694142, -97.184773), new(101.819117, -97.0363), new(102.756433, -97.677076), new(103.365688, -97.91932), new(104.174348, -97.761932), })),
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
                Boundary = new Polygon(new LinearRing(Array.Empty<Coordinate>())),
            }, CancellationToken.None);

        var terrain = result.Data!;
        Assert.That(terrain.Boundary, Is.EqualTo(new Polygon(new LinearRing(Array.Empty<Coordinate>()))));
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

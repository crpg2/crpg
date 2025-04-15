using Crpg.Application.Terrains.Commands;
using Crpg.Domain.Entities.Terrains;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Terrains;

public class CreateTerrainCommandTest : TestBase
{
    [Test]
    public async Task ShouldCreateTerrain()
    {
        var result = await new CreateTerrainCommand.Handler(ActDb, Mapper).Handle(
            new CreateTerrainCommand
            {
                Type = TerrainType.ThickForest,
                Boundary = new Polygon(new LinearRing(Array.Empty<Coordinate>())),
            }, CancellationToken.None);

        var terrain = result.Data!;
        Assert.That(terrain.Type, Is.EqualTo(TerrainType.ThickForest));
    }
}

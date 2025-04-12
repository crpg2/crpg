using Crpg.Application.Terrains.Queries;
using Crpg.Domain.Entities.Terrains;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Terrains;

public class GetTerrainsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnAllTerrains()
    {
        Terrain[] terrains =
            {
                new()
                {
                    Type = TerrainType.ThickForest,
                    Boundary = new Polygon(new LinearRing(Array.Empty<Coordinate>())),
                },
                new()
                {
                    Type = TerrainType.ShallowWater,
                    Boundary = new Polygon(new LinearRing(Array.Empty<Coordinate>())),
                },
            };

        ArrangeDb.Terrains.AddRange(terrains);
        await ArrangeDb.SaveChangesAsync();

        GetTerrainsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetTerrainsQuery(), CancellationToken.None);
        var terrainsViews = res.Data!;
        Assert.That(terrainsViews, Is.Not.Null);
        Assert.That(terrainsViews.Count, Is.EqualTo(2));

        Assert.That(terrainsViews[0].Type, Is.EqualTo(TerrainType.ThickForest));
        Assert.That(terrainsViews[1].Type, Is.EqualTo(TerrainType.ShallowWater));
    }
}

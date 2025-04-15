using Crpg.Application.Common.Results;
using Crpg.Application.Terrains.Commands;
using Crpg.Domain.Entities.Terrains;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Terrains;

public class DeleteTerrainCommandTest : TestBase
{
    [Test]
    public async Task ShouldDeleteTerrain()
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

        await new DeleteTerrainCommand.Handler(ActDb).Handle(
             new DeleteTerrainCommand
             {
                 Id = 1,
             }, CancellationToken.None);

        Assert.That(await AssertDb.Terrains.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task NotFoundTerrain()
    {
        var result = await new DeleteTerrainCommand.Handler(ActDb).Handle(
            new DeleteTerrainCommand
            {
                Id = 1,
            }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.TerrainNotFound));
    }
}

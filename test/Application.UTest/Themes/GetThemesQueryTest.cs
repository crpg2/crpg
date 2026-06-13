using Crpg.Application.Themes.Queries;
using Crpg.Domain.Entities.Themes;
using NUnit.Framework;

namespace Crpg.Application.UTest.Themes;

public class GetThemesQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnEmptyWhenNoThemes()
    {
        var handler = new GetThemesQuery.Handler(ActDb, Mapper);
        var result = await handler.Handle(new GetThemesQuery(), CancellationToken.None);

        Assert.That(result?.Data, Is.Empty);
    }

    [Test]
    public async Task ShouldReturnThemes()
    {
        var themeOne = CreateDefaultTheme(name: "Theme 1");
        var themeTwo = CreateDefaultTheme(name: "Theme 2");
        ArrangeDb.Themes.AddRange(themeOne, themeTwo);
        await ArrangeDb.SaveChangesAsync(CancellationToken.None);

        var handler = new GetThemesQuery.Handler(ActDb, Mapper);

        var result = await handler.Handle(new GetThemesQuery(), CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data, Has.Count.EqualTo(2));

        var resultOne = result.Data!.Single(x => x.Name == themeOne.Name);
        Assert.That(resultOne.Id, Is.GreaterThan(0));
        Assert.That(resultOne.Name, Is.EqualTo(themeOne.Name));

        var resultTwo = result.Data!.Single(x => x.Name == themeTwo.Name);
        Assert.That(resultTwo.Id, Is.GreaterThan(0));
        Assert.That(resultTwo.Name, Is.EqualTo(themeTwo.Name));
    }

    private static Theme CreateDefaultTheme(string name = "My Theme")
    {
        return new Theme(name);
    }
}

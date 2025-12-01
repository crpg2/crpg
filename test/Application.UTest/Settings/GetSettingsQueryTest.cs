using Crpg.Application.Settings.Queries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settings;

public class GetSettingsQueryTest : TestBase
{
    [Test]
    public async Task Base()
    {
        ArrangeDb.Settings.Add(new()
        {
            Id = 1,
            Discord = "link",
            Steam = "link",
            Patreon = "link",
            Github = "link",
            Reddit = "link",
            ModDb = "link",
            HappyHours = "some",
        });
        await ArrangeDb.SaveChangesAsync();

        var res = await new GetSettingsQuery.Handler(ActDb, Mapper).Handle(new GetSettingsQuery(), CancellationToken.None);

        var settingsViews = res.Data!;
        Assert.That(settingsViews, Is.Not.Null);
        Assert.That(settingsViews.Reddit, Is.EqualTo("link"));
    }
}

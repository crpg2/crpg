using Crpg.Application.ActivityLogs.Commands;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Sdk;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.ActivityLogs;

public class DeleteOldActivityLogsCommandTest : TestBase
{
    [Test]
    public async Task ShouldDeleteOldLogs()
    {
        ArrangeDb.ActivityLogs.AddRange(
        [
            new() { CreatedAt = DateTime.Now.AddDays(-1) },
            new() { CreatedAt = DateTime.Now.AddDays(-10) },
            new() { CreatedAt = DateTime.Now.AddDays(-29) },
            new() { CreatedAt = DateTime.Now.AddDays(-31), Metadata = { new ActivityLogMetadata("a", "b") } },
            new() { CreatedAt = DateTime.Now.AddDays(-50), Metadata = { new ActivityLogMetadata("c", "d") } },
        ]);
        await ArrangeDb.SaveChangesAsync();

        DeleteOldActivityLogsCommand.Handler handler = new(ActDb, new MachineDateTime());
        await handler.Handle(new DeleteOldActivityLogsCommand(), CancellationToken.None);

        Assert.That(await AssertDb.ActivityLogs.CountAsync(), Is.EqualTo(3));
        Assert.That(await AssertDb.ActivityLogMetadata.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldUse15DaysRetentionForCharacterEarned()
    {
        DateTime now = new(2026, 3, 31, 12, 0, 0, DateTimeKind.Utc);
        var dateTime = new Mock<IDateTime>();
        dateTime.Setup(d => d.UtcNow).Returns(now);

        ArrangeDb.ActivityLogs.AddRange(
        [
            new() { Type = ActivityLogType.CharacterEarned, CreatedAt = now.AddDays(-14) },
            new() { Type = ActivityLogType.CharacterEarned, CreatedAt = now.AddDays(-16) },
        ]);
        await ArrangeDb.SaveChangesAsync();

        DeleteOldActivityLogsCommand.Handler handler = new(ActDb, dateTime.Object);
        await handler.Handle(new DeleteOldActivityLogsCommand(), CancellationToken.None);

        ActivityLog[] logs = await AssertDb.ActivityLogs.ToArrayAsync();
        Assert.That(logs.Length, Is.EqualTo(1));
        Assert.That(logs[0].Type, Is.EqualTo(ActivityLogType.CharacterEarned));
        Assert.That(logs[0].CreatedAt, Is.EqualTo(now.AddDays(-14)));
    }

    [Test]
    public async Task ShouldUse180DaysRetentionForMarketplaceOfferCreated()
    {
        DateTime now = new(2026, 3, 31, 12, 0, 0, DateTimeKind.Utc);
        var dateTime = new Mock<IDateTime>();
        dateTime.Setup(d => d.UtcNow).Returns(now);

        ArrangeDb.ActivityLogs.AddRange(
        [
            new() { Type = ActivityLogType.MarketplaceOfferCreated, CreatedAt = now.AddDays(-179) },
            new() { Type = ActivityLogType.MarketplaceOfferCreated, CreatedAt = now.AddDays(-181) },
        ]);
        await ArrangeDb.SaveChangesAsync();

        DeleteOldActivityLogsCommand.Handler handler = new(ActDb, dateTime.Object);
        await handler.Handle(new DeleteOldActivityLogsCommand(), CancellationToken.None);

        ActivityLog[] logs = await AssertDb.ActivityLogs.ToArrayAsync();
        Assert.That(logs.Length, Is.EqualTo(1));
        Assert.That(logs[0].Type, Is.EqualTo(ActivityLogType.MarketplaceOfferCreated));
        Assert.That(logs[0].CreatedAt, Is.EqualTo(now.AddDays(-179)));
    }
}

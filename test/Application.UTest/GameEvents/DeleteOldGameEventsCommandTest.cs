using Crpg.Application.GameEvents.Commands;
using Crpg.Domain.Entities.GameEvents;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.GameEvents;

public class DeleteOldGameEventsCommandTest : TestBase
{
    [Test]
    public async Task ShouldDeleteEventsOlderThan30Days()
    {
        DateTime now = new(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        var dateTime = new Mock<IDateTime>();
        dateTime.Setup(d => d.UtcNow).Returns(now);

        ArrangeDb.GameEvents.AddRange(
        [
            new GameEvent { CreatedAt = now.AddDays(-1), Type = GameEventType.Kill },
            new GameEvent { CreatedAt = now.AddDays(-13), Type = GameEventType.Hit },
            new GameEvent { CreatedAt = now.AddDays(-30), Type = GameEventType.Block },
            new GameEvent { CreatedAt = now.AddDays(-31), Type = GameEventType.Kill },
        ]);
        await ArrangeDb.SaveChangesAsync();

        DeleteOldGameEventsCommand.Handler handler = new(ActDb, dateTime.Object);
        await handler.Handle(new DeleteOldGameEventsCommand(), CancellationToken.None);

        var remaining = await AssertDb.GameEvents.ToArrayAsync();
        Assert.That(remaining.Length, Is.EqualTo(3));
        Assert.That(remaining.All(e => e.CreatedAt >= now.AddDays(-30)), Is.True);
    }
}

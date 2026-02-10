using Crpg.Application.IdempotencyKeys.Commands;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.GameServers;
using Crpg.Sdk;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.IdempotencyKeys;

public class DeleteOldIdempotencyKeysCommandTest : TestBase
{
    [Test]
    public async Task ShouldDeleteOldKeys()
    {
        ArrangeDb.IdempotencyKeys.AddRange(new IdempotencyKey[]
        {
            new() { CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new() { CreatedAt = DateTime.UtcNow.AddHours(-10) },
            new() { CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { CreatedAt = DateTime.UtcNow.AddHours(-25) },
            new() { CreatedAt = DateTime.UtcNow.AddDays(-30) },
        });
        await ArrangeDb.SaveChangesAsync();

        DeleteOldIdempotencyKeysCommand.Handler handler = new(ActDb, new MachineDateTime());
        await handler.Handle(new DeleteOldIdempotencyKeysCommand(), CancellationToken.None);

        Assert.That(await AssertDb.IdempotencyKeys.CountAsync(), Is.EqualTo(2));
    }
}

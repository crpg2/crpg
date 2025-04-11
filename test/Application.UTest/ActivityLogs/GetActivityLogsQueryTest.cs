using Crpg.Application.ActivityLogs.Queries;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.ActivityLogs;

public class GetActivityLogsQueryTest : TestBase
{
    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };

    [Test]
    public async Task ShouldReturnAllLogsWithNoUserIdsAndNoTypes()
    {
        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-6) },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-8) },
        });
        await ArrangeDb.SaveChangesAsync();

        GetActivityLogsQuery.Handler handler = new(ActDb, Mapper, ActivityLogService.Object);
        var res = await handler.Handle(new GetActivityLogsQuery
        {
            From = DateTime.UtcNow.AddMinutes(-7),
            To = DateTime.UtcNow.AddMinutes(-3),
            UserIds = Array.Empty<int>(),
            Types = Array.Empty<ActivityLogType>(),
        }, CancellationToken.None);

        Assert.That(res.Data!.ActivityLogs!.Count, Is.EqualTo(2));
        Assert.That(res.Data!.ActivityLogs[0].Id, Is.EqualTo(2));
        Assert.That(res.Data!.ActivityLogs[1].Id, Is.EqualTo(3));
    }

    [Test]
    public async Task ShouldReturnLogsForUserId()
    {
        User user = new();
        ArrangeDb.Users.Add(user);

        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { User = user, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new() { User = user, CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new() { User = new User(), CreatedAt = DateTime.UtcNow.AddMinutes(-6) },
            new() { User = user, CreatedAt = DateTime.UtcNow.AddMinutes(-8) },
        });
        await ArrangeDb.SaveChangesAsync();

        GetActivityLogsQuery.Handler handler = new(ActDb, Mapper, ActivityLogService.Object);
        var res = await handler.Handle(new GetActivityLogsQuery
        {
            From = DateTime.UtcNow.AddMinutes(-10),
            To = DateTime.UtcNow,
            UserIds = new[] { user.Id },
            Types = Array.Empty<ActivityLogType>(),
        }, CancellationToken.None);

        Assert.That(res.Data!.ActivityLogs.Count, Is.EqualTo(3));
        Assert.That(res.Data!.ActivityLogs[0].Id, Is.EqualTo(1));
        Assert.That(res.Data!.ActivityLogs[1].Id, Is.EqualTo(2));
        Assert.That(res.Data!.ActivityLogs[2].Id, Is.EqualTo(4));
    }

    [Test]
    public async Task ShouldReturnLogsForType()
    {
        User user = new();
        ArrangeDb.Users.Add(user);

        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { User = user, Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new() { User = user, Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new() { User = user, Type = ActivityLogType.UserDeleted, CreatedAt = DateTime.UtcNow.AddMinutes(-6) },
            new() { User = user, Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-8) },
        });
        await ArrangeDb.SaveChangesAsync();

        GetActivityLogsQuery.Handler handler = new(ActDb, Mapper, ActivityLogService.Object);
        var res = await handler.Handle(new GetActivityLogsQuery
        {
            From = DateTime.UtcNow.AddMinutes(-10),
            To = DateTime.UtcNow,
            UserIds = new[] { user.Id },
            Types = new[] { ActivityLogType.UserCreated },
        }, CancellationToken.None);

        Assert.That(res.Data!.ActivityLogs!.Count, Is.EqualTo(3));
        Assert.That(res.Data!.ActivityLogs[0].Id, Is.EqualTo(1));
        Assert.That(res.Data!.ActivityLogs[1].Id, Is.EqualTo(2));
        Assert.That(res.Data!.ActivityLogs[2].Id, Is.EqualTo(4));
    }

    [Test]
    public async Task ShouldReturnLogsForUserAndType()
    {
        User user = new();
        ArrangeDb.Users.Add(user);

        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { User = new User(), Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new() { User = user, Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new() { User = user, Type = ActivityLogType.UserDeleted, CreatedAt = DateTime.UtcNow.AddMinutes(-6) },
            new() { User = new User(), Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-8) },
        });
        await ArrangeDb.SaveChangesAsync();

        GetActivityLogsQuery.Handler handler = new(ActDb, Mapper, ActivityLogService.Object);
        var res = await handler.Handle(new GetActivityLogsQuery
        {
            From = DateTime.UtcNow.AddMinutes(-10),
            To = DateTime.UtcNow,
            UserIds = new[] { user.Id },
            Types = new[] { ActivityLogType.UserDeleted },
        }, CancellationToken.None);

        Assert.That(res.Data!.ActivityLogs!.Count, Is.EqualTo(1));
        Assert.That(res.Data!.ActivityLogs[0].Id, Is.EqualTo(3));
    }

    [Test]
    public async Task ShouldReturnEntitiesFromMetadataInDict()
    {
        ActivityLog log = new();
        ArrangeDb.ActivityLogs.Add(log);

        Clan clan = new() { Id = 10 };
        User user = new() { Id = 20 };
        Character character = new() { Id = 30 };

        ArrangeDb.Clans.Add(clan);
        ArrangeDb.Users.Add(user);
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        EntitiesFromMetadata extractedEntities = new()
        {
            ClansIds = new List<int> { 10 },
            UsersIds = new List<int> { 20 },
            CharactersIds = new List<int> { 30 },
        };

        ActivityLogService.Setup(s => s.ExtractEntitiesFromMetadata(It.IsAny<ActivityLog[]>()))
            .Returns(extractedEntities);

        var query = new GetActivityLogsQuery
        {
            From = DateTime.UtcNow.AddMinutes(-10),
            To = DateTime.UtcNow,
            UserIds = Array.Empty<int>(),
            Types = Array.Empty<ActivityLogType>(),
        };

        var handler = new GetActivityLogsQuery.Handler(ActDb, Mapper, ActivityLogService.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null, "The result data must not be null");

        var dict = result.Data!.Dict;
        Assert.That(dict, Is.Not.Null, "The dictionary must be initialized");

        Assert.That(dict.Clans.Count, Is.EqualTo(1), "It is expected that 1 clan will be found");
        Assert.That(dict.Clans[0].Id, Is.EqualTo(10), "Clan ID is not as expected");

        Assert.That(dict.Users.Count, Is.EqualTo(1), "It is expected that 1 user will be found");
        Assert.That(dict.Users[0].Id, Is.EqualTo(20), "User ID does not match the expected user ID");

        Assert.That(dict.Characters.Count, Is.EqualTo(1), "1 character is expected to be found");
        Assert.That(dict.Characters[0].Id, Is.EqualTo(30), "The character ID is not as expected");
    }
}

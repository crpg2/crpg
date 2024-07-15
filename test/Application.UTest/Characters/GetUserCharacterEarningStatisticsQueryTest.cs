using Crpg.Application.Characters.Queries;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class GetUserCharacterEarningStatisticsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfCharacterDoesntExist()
    {
        User user1 = new();
        User user2 = new();
        Character character1 = new() { User = user1 };
        Character character2 = new() { User = user2 };
        Character character3 = new() { User = user1 };
        ArrangeDb.Users.AddRange(user1, user2);
        ArrangeDb.Characters.AddRange(character1, character2, character3);
        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { User = user1, Type = ActivityLogType.CharacterEarned, CreatedAt = DateTime.UtcNow.AddMinutes(-10),  Metadata = { new("characterId", character1.Id.ToString()), new("gameMode", "CRPGBattle"), new("experience", "1000"), new("gold", "50") }, },
            new() { User = user1, Type = ActivityLogType.CharacterEarned, CreatedAt = DateTime.UtcNow.AddMinutes(-10),  Metadata = { new("characterId", character3.Id.ToString()), new("gameMode", "CRPGBattle"), new("experience", "1000"), new("gold", "50") }, },    // another character
            new() { User = user1, Type = ActivityLogType.CharacterEarned, CreatedAt = DateTime.UtcNow.AddMinutes(-30),  Metadata = { new("characterId", character1.Id.ToString()), new("gameMode", "CRPGBattle"), new("experience", "1000"), new("gold", "50") }, },    // old log record
            new() { User = user2, Type = ActivityLogType.CharacterEarned, CreatedAt = DateTime.UtcNow.AddMinutes(-30),  Metadata = { new("characterId", character2.Id.ToString()), new("gameMode", "CRPGBattle"), new("experience", "1000"), new("gold", "50") }, },    // another user
            new() { User = user1, Type = ActivityLogType.CharacterCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-20), Metadata = { new("foo", "bar") } }, // another log type
        });
        await ArrangeDb.SaveChangesAsync();

        GetUserCharacterEarningStatisticsQuery.Handler handler = new(ActDb, Mapper,  new MachineDateTime());
        var res = await handler.Handle(new GetUserCharacterEarningStatisticsQuery
        {
            From = DateTime.UtcNow.AddMinutes(-20),
            CharacterId = character1.Id,
            UserId = user1.Id,
        }, CancellationToken.None);

        Assert.That(res.Data!.Count, Is.EqualTo(1));
        Assert.That(res.Data!.First().UserId, Is.EqualTo(user1.Id));
    }
}

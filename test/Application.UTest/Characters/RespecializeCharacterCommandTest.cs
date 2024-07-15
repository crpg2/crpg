using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Limitations;
using Crpg.Domain.Entities.Servers;
using Crpg.Domain.Entities.Users;
using Crpg.Persistence;
using Crpg.Sdk;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class RespecializeCharacterCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        RespecializePriceHalfLife = 12,
        RespecializePriceForLevel30 = 5000,
        FreeRespecializeIntervalDays = 7,
        FreeRespecializePostWindowHours = 12,
    };

    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly IExperienceTable ExperienceTable = Mock.Of<IExperienceTable>();
    private static readonly ICharacterService CharacterService = Mock.Of<ICharacterService>();
    private static readonly IUserService UserService = Mock.Of<IUserService>();

    [Theory]
    public async Task RespecializeCharacterShouldReduceGoldAndUpdateLastRespecTime(bool freeRespec)
    {
        Character character = new()
        {
            Generation = 2,
            Level = 3,
            Experience = 150,
            ForTournament = false,
            EquippedItems =
            {
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Head },
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Body },
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Weapon0 },
            },
            Statistics = new List<CharacterStatistics>
            {
                {
                    new CharacterStatistics
                    {
                        Kills = 1,
                        Deaths = 2,
                        Assists = 3,
                        PlayTime = TimeSpan.FromSeconds(4),
                        GameMode = GameMode.CRPGBattle,
                    }
                },
            },
            User = new() { Gold = 1000 },
            Limitations = new CharacterLimitations
            {
                LastRespecializeAt = freeRespec
                    ? new DateTime(2023, 3, 9)
                    : new DateTime(2023, 3, 16),
            },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<IExperienceTable> experienceTableMock = new();
        experienceTableMock.Setup(et => et.GetLevelForExperience(75)).Returns(2);
        experienceTableMock.Setup(et => et.GetExperienceForLevel(30)).Returns(100000);
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2023, 3, 17));

        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object,
            UserService, experienceTableMock.Object, ActivityLogService.Object, dateTimeMock.Object, Constants);
        await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        character = await AssertDb.Characters
            .Include(c => c.User)
            .Include(c => c.EquippedItems)
            .FirstAsync(c => c.Id == character.Id);
        var characterDb = await AssertDb.Characters
            .Include(c => c.Limitations)
            .FirstAsync(c => c.Id == character.Id);
        if (freeRespec)
        {
            Assert.That(character.User!.Gold, Is.EqualTo(1000));
            Assert.That(characterDb.Limitations!.LastRespecializeAt, Is.EqualTo(dateTimeMock.Object.UtcNow));
        }
        else
        {
            Assert.That(character.User!.Gold, Is.LessThan(1000));
            Assert.That(characterDb.Limitations!.LastRespecializeAt, Is.EqualTo(new DateTime(2023, 3, 17)));
        }

        Assert.That(character.Generation, Is.EqualTo(2));
        Assert.That(character.Level, Is.EqualTo(3));
        Assert.That(character.Experience, Is.EqualTo(150));
        Assert.That(character.EquippedItems.Count, Is.EqualTo(3));
        CharacterStatistics? charStats = character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle);
        Assert.That(charStats?.Kills, Is.EqualTo(1));
        Assert.That(charStats?.Deaths, Is.EqualTo(2));
        Assert.That(charStats?.Assists, Is.EqualTo(3));
        Assert.That(charStats?.PlayTime, Is.EqualTo(TimeSpan.FromSeconds(4)));

        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), true));
    }

    [Test]
    public async Task RespecializeTournamentCharacterShouldNotChangeLevelOrGold()
    {
        Character character = new()
        {
            Generation = 0,
            Level = 3,
            Experience = 150,
            ForTournament = true,
            EquippedItems =
            {
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Head },
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Body },
                new EquippedItem { UserItem = new UserItem(), Slot = ItemSlot.Weapon0 },
            },
            Statistics = new List<CharacterStatistics>
            {
                {
                    new CharacterStatistics
                    {
                        Kills = 1,
                        Deaths = 2,
                        Assists = 3,
                        PlayTime = TimeSpan.FromSeconds(4),
                        GameMode = GameMode.CRPGBattle,
                    }
                },
            },
            User = new() { Gold = 500 },
            Limitations = new CharacterLimitations { LastRespecializeAt = DateTime.UtcNow - TimeSpan.FromDays(1) },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();
        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object,
         UserService, ExperienceTable, ActivityLogService.Object, new MachineDateTime(), Constants);
        await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        character = await AssertDb.Characters
            .Include(c => c.User)
            .Include(c => c.EquippedItems)
            .FirstAsync(c => c.Id == character.Id);
        Assert.That(character.User!.Gold, Is.EqualTo(500));
        Assert.That(character.Generation, Is.EqualTo(0));
        Assert.That(character.Level, Is.EqualTo(3));
        Assert.That(character.Experience, Is.EqualTo(150));
        Assert.That(character.EquippedItems.Count, Is.EqualTo(3));
        CharacterStatistics? charStats = character.Statistics.FirstOrDefault(s => s.GameMode == GameMode.CRPGBattle);
        Assert.That(charStats?.Kills, Is.EqualTo(1));
        Assert.That(charStats?.Deaths, Is.EqualTo(2));
        Assert.That(charStats?.Assists, Is.EqualTo(3));
        Assert.That(charStats?.PlayTime, Is.EqualTo(TimeSpan.FromSeconds(4)));

        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), true));
    }

    [Test]
    public async Task RespecializeCharacterForRecentUserShouldNotChangeGold()
    {
        Character character = new()
        {
            ForTournament = false,
            User = new() { Gold = 500 },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();
        Mock<IUserService> userServiceMock = new();
        userServiceMock
            .Setup(us => us.CheckIsRecentUser(It.IsAny<CrpgDbContext>(), It.IsAny<User>()))
            .Returns(Task.FromResult(true));

        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object,
         userServiceMock.Object, ExperienceTable, ActivityLogService.Object, new MachineDateTime(), Constants);
        await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        character = await AssertDb.Characters
            .Include(c => c.User)
            .FirstAsync(c => c.Id == character.Id);

        Assert.That(character.User!.Gold, Is.EqualTo(500));
        userServiceMock.Verify(cs => cs.CheckIsRecentUser(It.IsAny<CrpgDbContext>(), It.IsAny<User>()));
        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), true));
    }

    [Test]
    public async Task RespecializeCharacterInFreeRespecPostWindowShouldNotChangeGold()
    {
        Character character = new()
        {
            ForTournament = false,
            User = new() { Gold = 500 },
            Limitations = new CharacterLimitations { LastRespecializeAt = DateTime.UtcNow - TimeSpan.FromHours(11) },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();

        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object,
         UserService, ExperienceTable, ActivityLogService.Object, new MachineDateTime(), Constants);
        await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        character = await AssertDb.Characters
            .Include(c => c.User)
            .FirstAsync(c => c.Id == character.Id);

        Assert.That(character.User!.Gold, Is.EqualTo(500));
        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), true));
    }

    [Test]
    public async Task ShouldReturnErrorIfNoEnoughGold()
    {
        Character character = new()
        {
            Generation = 2,
            Level = 3,
            Experience = 150,
            ForTournament = false,
            User = new() { Gold = 0 },
            Limitations = new CharacterLimitations { LastRespecializeAt = new DateTime(2023, 3, 16, 0, 0, 0) },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<IExperienceTable> experienceTableMock = new();
        experienceTableMock.Setup(et => et.GetLevelForExperience(75)).Returns(2);
        experienceTableMock.Setup(et => et.GetExperienceForLevel(30)).Returns(100000);
        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2023, 3, 16, 13, 0, 0));

        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, CharacterService,
            UserService, experienceTableMock.Object, ActivityLogService.Object, dateTimeMock.Object, Constants);
        var res = await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughGold));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfUserDoesntExist()
    {
        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, CharacterService, UserService,
         ExperienceTable, ActivityLogService.Object, new MachineDateTime(), Constants);
        var result = await handler.Handle(
            new RespecializeCharacterCommand
            {
                CharacterId = 1,
                UserId = 2,
            }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task ShouldReturnNotFoundIfCharacterDoesntExist()
    {
        var user = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        RespecializeCharacterCommand.Handler handler = new(ActDb, Mapper, CharacterService, UserService,
         ExperienceTable, ActivityLogService.Object, new MachineDateTime(), Constants);
        var result = await handler.Handle(new RespecializeCharacterCommand
        {
            CharacterId = 1,
            UserId = user.Entity.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }
}

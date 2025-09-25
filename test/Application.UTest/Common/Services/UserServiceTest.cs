using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class UserServiceTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        DefaultGold = 300,
        DefaultHeirloomPoints = 0,
        DefaultExperienceMultiplier = 1.0f,
        NewUserStartingCharacterLevel = 30,
    };

    [Test]
    public void SetDefaultValuesShouldSetDefaultValues()
    {
        UserService userService = new(Mock.Of<IDateTime>(), Constants);
        User user = new();
        userService.SetDefaultValuesForUser(user);

        Assert.That(user.Gold, Is.EqualTo(Constants.DefaultGold));
        Assert.That(user.Role, Is.EqualTo(Role.User));
        Assert.That(user.HeirloomPoints, Is.EqualTo(Constants.DefaultHeirloomPoints));
        Assert.That(user.ExperienceMultiplier, Is.EqualTo(Constants.DefaultExperienceMultiplier));
    }

    [Test]
    public void SetDefaultValuesShouldGiveGoldIsUserIsBeingCreated()
    {
        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(DateTime.UtcNow);

        UserService userService = new(dateTimeMock.Object, Constants);
        User user = new() { CreatedAt = default };
        userService.SetDefaultValuesForUser(user);
        Assert.That(user.Gold, Is.EqualTo(Constants.DefaultGold));
    }

    [Test]
    public void SetDefaultValuesShouldGiveGoldIfTheUserWasCreatedSomeTimeAgo()
    {
        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 10, 1));

        UserService userService = new(dateTimeMock.Object, Constants);
        User user = new() { CreatedAt = new DateTime(2000, 8, 1) };
        userService.SetDefaultValuesForUser(user);
        Assert.That(user.Gold, Is.EqualTo(Constants.DefaultGold));
    }

    [TestCase(100, 100)]
    [TestCase(500, 300)]
    public void SetDefaultValuesShouldNotGiveGoldIfTheUserWasCreatedRecently(int currentGold, int expectedGold)
    {
        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 10, 1));

        UserService userService = new(dateTimeMock.Object, Constants);
        User user = new() { Gold = currentGold, CreatedAt = new DateTime(2000, 10, 5) };
        userService.SetDefaultValuesForUser(user);
        Assert.That(user.Gold, Is.EqualTo(expectedGold));
    }

    [TestCase(1.0f, 29, 1000, true)]
    [TestCase(1.1f, 30, 1000, false)]
    public async Task VeteranUserWithSingleHighLevelCharacterNotBeConsiderRecent(float experienceMultiplier, int characterLevel, int characterExperience, bool expectedResult)
    {
        User user = new() { Id = 1, ExperienceMultiplier = experienceMultiplier };
        Character character = new() { UserId = 1, Level = characterLevel, Experience = characterExperience };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 10, 1));

        UserService userService = new(dateTimeMock.Object, Constants);

        Assert.That(await userService.CheckIsRecentUser(ActDb, user.Id, CancellationToken.None), Is.EqualTo(expectedResult));
    }

    [Test]
    public async Task VeteranUserWithMultiplyHighLevelCharactersNotBeConsiderRecent()
    {
        User user = new() { Id = 1, ExperienceMultiplier = 1 };
        Character[] characters =
        {
            new() { UserId = 1, Level = 29, Experience = 4000000 },
            new() { UserId = 1, Level = 29, Experience = 4000000 },
            new() { UserId = 1, Level = 29, Experience = 4000001 },
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Characters.AddRange(characters);

        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 10, 1));

        UserService userService = new(dateTimeMock.Object, Constants);

        Assert.That(await userService.CheckIsRecentUser(ActDb, user.Id, CancellationToken.None), Is.False);
    }
}

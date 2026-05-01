using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Quests.Commands;
using Crpg.Application.Quests.Services;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Notifications;
using Crpg.Domain.Entities.Quests;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Quests;

public class ClaimQuestRewardCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        QuestDailyQuestsPerUser = 3,
        QuestWeeklyQuestsPerUser = 1,
        QuestRerollDailyQuestPrice = 300,
    };

    [Test]
    public async Task ShouldReturnErrorIfUserQuestNotFound()
    {
        Mock<IDateTime> dateTimeMock = new();
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestEvaluationService> questEvaluationServiceMock = new();

        var handler = new ClaimQuestRewardCommand.Handler(
            ActDb,
            Mapper,
            dateTimeMock.Object,
            characterServiceMock.Object,
            activityLogServiceMock.Object,
            questEvaluationServiceMock.Object);

        var result = await handler.Handle(new ClaimQuestRewardCommand
        {
            UserId = 1,
            UserQuestId = 2,
            CharacterId = 3,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserQuestNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfQuestRewardAlreadyClaimed()
    {
        var user = new User { Id = 1, Gold = 1000 };
        var questDefinition = new QuestDefinition
        {
            Id = 1,
            RequiredValue = 10,
            RewardGold = 100,
            RewardExperience = 200,
        };
        var userQuest = new UserQuest
        {
            Id = 2,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = questDefinition.Id,
            QuestDefinition = questDefinition,
            IsRewardClaimed = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.QuestDefinitions.Add(questDefinition);
        ArrangeDb.UserQuests.Add(userQuest);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestEvaluationService> questEvaluationServiceMock = new();

        var handler = new ClaimQuestRewardCommand.Handler(
            ActDb,
            Mapper,
            dateTimeMock.Object,
            characterServiceMock.Object,
            activityLogServiceMock.Object,
            questEvaluationServiceMock.Object);

        var result = await handler.Handle(new ClaimQuestRewardCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
            CharacterId = 3,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.QuestRewardAlreadyClaimed));
    }

    [Test]
    public async Task ShouldReturnErrorIfQuestExpired()
    {
        var user = new User { Id = 1, Gold = 1000 };
        var questDefinition = new QuestDefinition
        {
            Id = 1,
            RequiredValue = 10,
            RewardGold = 100,
            RewardExperience = 200,
        };
        var userQuest = new UserQuest
        {
            Id = 2,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = questDefinition.Id,
            QuestDefinition = questDefinition,
            IsRewardClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // expired
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.QuestDefinitions.Add(questDefinition);
        ArrangeDb.UserQuests.Add(userQuest);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestEvaluationService> questEvaluationServiceMock = new();

        var handler = new ClaimQuestRewardCommand.Handler(
            ActDb,
            Mapper,
            dateTimeMock.Object,
            characterServiceMock.Object,
            activityLogServiceMock.Object,
            questEvaluationServiceMock.Object);

        var result = await handler.Handle(new ClaimQuestRewardCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
            CharacterId = 3,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.QuestExpired));
    }

    [Test]
    public async Task ShouldReturnErrorIfQuestDefinitionNotFound()
    {
        var user = new User { Id = 1, Gold = 1000 };
        // Quest definition not added to db
        var userQuest = new UserQuest
        {
            Id = 2,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = 999,
            IsRewardClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.UserQuests.Add(userQuest);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestEvaluationService> questEvaluationServiceMock = new();

        var handler = new ClaimQuestRewardCommand.Handler(
            ActDb,
            Mapper,
            dateTimeMock.Object,
            characterServiceMock.Object,
            activityLogServiceMock.Object,
            questEvaluationServiceMock.Object);

        var result = await handler.Handle(new ClaimQuestRewardCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
            CharacterId = 3,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserQuestNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfQuestNotCompleted()
    {
        var user = new User { Id = 1, Gold = 1000 };
        var questDefinition = new QuestDefinition
        {
            Id = 1,
            RequiredValue = 10,
            RewardGold = 100,
            RewardExperience = 200,
        };
        var userQuest = new UserQuest
        {
            Id = 2,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = questDefinition.Id,
            QuestDefinition = questDefinition,
            IsRewardClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.QuestDefinitions.Add(questDefinition);
        ArrangeDb.UserQuests.Add(userQuest);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestEvaluationService> questEvaluationServiceMock = new();
        questEvaluationServiceMock.Setup(q => q.ComputeCurrentValuesAsync(It.IsAny<List<UserQuest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<UserQuest> uqs, CancellationToken ct) =>
                uqs.ToDictionary(uq => uq.Id, uq => 5)); // less than required 10

        var handler = new ClaimQuestRewardCommand.Handler(
            ActDb,
            Mapper,
            dateTimeMock.Object,
            characterServiceMock.Object,
            activityLogServiceMock.Object,
            questEvaluationServiceMock.Object);

        var result = await handler.Handle(new ClaimQuestRewardCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
            CharacterId = 3,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.QuestNotCompleted));
    }

    [Test]
    public async Task ShouldReturnErrorIfCharacterNotFound()
    {
        var user = new User { Id = 1, Gold = 1000 };
        var questDefinition = new QuestDefinition
        {
            Id = 1,
            RequiredValue = 10,
            RewardGold = 100,
            RewardExperience = 200,
        };
        var userQuest = new UserQuest
        {
            Id = 2,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = questDefinition.Id,
            QuestDefinition = questDefinition,
            IsRewardClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.QuestDefinitions.Add(questDefinition);
        ArrangeDb.UserQuests.Add(userQuest);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestEvaluationService> questEvaluationServiceMock = new();
        questEvaluationServiceMock.Setup(q => q.ComputeCurrentValuesAsync(It.IsAny<List<UserQuest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<UserQuest> uqs, CancellationToken ct) =>
                uqs.ToDictionary(uq => uq.Id, uq => 15)); // greater than required

        var handler = new ClaimQuestRewardCommand.Handler(
            ActDb,
            Mapper,
            dateTimeMock.Object,
            characterServiceMock.Object,
            activityLogServiceMock.Object,
            questEvaluationServiceMock.Object);

        var result = await handler.Handle(new ClaimQuestRewardCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
            CharacterId = 999, // non-existent character
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task ShouldClaimRewardSuccessfully()
    {
        var user = new User { Id = 1, Gold = 1000 };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var character = new Character { Id = 3, UserId = user.Id, User = user, Level = 5, Experience = 0 };
        var questDefinition = new QuestDefinition
        {
            Id = 1,
            RequiredValue = 10,
            RewardGold = 100,
            RewardExperience = 200,
        };
        var userQuest = new UserQuest
        {
            Id = 2,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = questDefinition.Id,
            QuestDefinition = questDefinition,
            IsRewardClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow.Date,
        };
        ArrangeDb.Characters.Add(character);
        ArrangeDb.QuestDefinitions.Add(questDefinition);
        ArrangeDb.UserQuests.Add(userQuest);
        await ArrangeDb.SaveChangesAsync();

        var now = DateTime.UtcNow;
        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(d => d.UtcNow).Returns(now);
        Mock<ICharacterService> characterServiceMock = new();
        Mock<IActivityLogService> activityLogServiceMock = new();
        activityLogServiceMock.Setup(al => al.CreateQuestRewardClaimedLog(user.Id, character.Id, userQuest.Id, 100, 200))
            .Returns(new ActivityLog());
        Mock<IQuestEvaluationService> questEvaluationServiceMock = new();
        questEvaluationServiceMock.Setup(q => q.ComputeCurrentValuesAsync(It.IsAny<List<UserQuest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<UserQuest> uqs, CancellationToken ct) =>
                uqs.ToDictionary(uq => uq.Id, uq => 15)); // greater than required

        var handler = new ClaimQuestRewardCommand.Handler(
            ActDb,
            Mapper,
            dateTimeMock.Object,
            characterServiceMock.Object,
            activityLogServiceMock.Object,
            questEvaluationServiceMock.Object);

        var result = await handler.Handle(new ClaimQuestRewardCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
            CharacterId = character.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        var vm = result.Data!;
        Assert.That(vm.Id, Is.EqualTo(userQuest.Id));
        Assert.That(vm.CurrentValue, Is.EqualTo(10)); // capped at required value

        var dbUser = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
        Assert.That(dbUser.Gold, Is.EqualTo(1100)); // 1000 + 100 reward

        characterServiceMock.Verify(cs => cs.GiveExperience(It.IsAny<Character>(), 200, false), Times.Once);

        var dbUserQuest = await AssertDb.UserQuests.FirstAsync(uq => uq.Id == userQuest.Id);
        Assert.That(dbUserQuest.IsRewardClaimed, Is.True);

        activityLogServiceMock.Verify(al => al.CreateQuestRewardClaimedLog(
            user.Id, character.Id, userQuest.Id, 100, 200), Times.Once);
    }
}

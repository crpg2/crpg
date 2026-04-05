using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Quests.Commands;
using Crpg.Application.Quests.Services;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Quests;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Quests;

public class RerollQuestCommandTest : TestBase
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
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestAssignmentService> questAssignmentServiceMock = new();

        var handler = new RerollQuestCommand.Handler(
            ActDb,
            dateTimeMock.Object,
            activityLogServiceMock.Object,
            questAssignmentServiceMock.Object,
            Constants);

        var result = await handler.Handle(new RerollQuestCommand
        {
            UserId = 1,
            UserQuestId = 2,
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
            Type = QuestType.Daily,
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
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestAssignmentService> questAssignmentServiceMock = new();

        var handler = new RerollQuestCommand.Handler(
            ActDb,
            dateTimeMock.Object,
            activityLogServiceMock.Object,
            questAssignmentServiceMock.Object,
            Constants);

        var result = await handler.Handle(new RerollQuestCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
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
            Type = QuestType.Daily,
        };
        var userQuest = new UserQuest
        {
            Id = 2,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = questDefinition.Id,
            QuestDefinition = questDefinition,
            IsRewardClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.QuestDefinitions.Add(questDefinition);
        ArrangeDb.UserQuests.Add(userQuest);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestAssignmentService> questAssignmentServiceMock = new();

        var handler = new RerollQuestCommand.Handler(
            ActDb,
            dateTimeMock.Object,
            activityLogServiceMock.Object,
            questAssignmentServiceMock.Object,
            Constants);

        var result = await handler.Handle(new RerollQuestCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.QuestExpired));
    }

    [Test]
    public async Task ShouldReturnErrorIfQuestDefinitionNotFound()
    {
        var user = new User { Id = 1, Gold = 1000 };
        var userQuest = new UserQuest
        {
            Id = 2,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = 999,
            QuestDefinition = null,
            IsRewardClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.UserQuests.Add(userQuest);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.UtcNow);
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestAssignmentService> questAssignmentServiceMock = new();

        var handler = new RerollQuestCommand.Handler(
            ActDb,
            dateTimeMock.Object,
            activityLogServiceMock.Object,
            questAssignmentServiceMock.Object,
            Constants);

        var result = await handler.Handle(new RerollQuestCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserQuestNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfQuestNotDaily()
    {
        var user = new User { Id = 1, Gold = 1000 };
        var questDefinition = new QuestDefinition
        {
            Id = 1,
            Type = QuestType.Weekly, // not daily
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
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestAssignmentService> questAssignmentServiceMock = new();

        var handler = new RerollQuestCommand.Handler(
            ActDb,
            dateTimeMock.Object,
            activityLogServiceMock.Object,
            questAssignmentServiceMock.Object,
            Constants);

        var result = await handler.Handle(new RerollQuestCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserQuestNotFound)); // because it filters on daily type
    }

    [Test]
    public async Task ShouldReturnErrorIfNotEnoughGold()
    {
        var user = new User { Id = 1, Gold = 200 }; // less than 300
        var questDefinition = new QuestDefinition
        {
            Id = 1,
            Type = QuestType.Daily,
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
        Mock<IActivityLogService> activityLogServiceMock = new();
        Mock<IQuestAssignmentService> questAssignmentServiceMock = new();

        var handler = new RerollQuestCommand.Handler(
            ActDb,
            dateTimeMock.Object,
            activityLogServiceMock.Object,
            questAssignmentServiceMock.Object,
            Constants);

        var result = await handler.Handle(new RerollQuestCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughGold));
    }

    [Test]
    public async Task ShouldRerollQuestSuccessfully()
    {
        var user = new User { Id = 1, Gold = 1000 };
        var questDefinition = new QuestDefinition
        {
            Id = 1,
            Type = QuestType.Daily,
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
        ArrangeDb.Users.Add(user);
        ArrangeDb.QuestDefinitions.Add(questDefinition);
        ArrangeDb.UserQuests.Add(userQuest);
        await ArrangeDb.SaveChangesAsync();

        var newUserQuest = new UserQuest
        {
            Id = 3,
            UserId = user.Id,
            QuestDefinitionId = 5,
            IsRewardClaimed = false,
            ExpiresAt = userQuest.ExpiresAt,
        };

        DateTime now = DateTime.UtcNow;
        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(d => d.UtcNow).Returns(now);
        Mock<IActivityLogService> activityLogServiceMock = new();
        activityLogServiceMock.Setup(al => al.CreateQuestRerolledLog(user.Id, userQuest.Id, newUserQuest.Id, Constants.QuestRerollDailyQuestPrice))
            .Returns(new ActivityLog());
        Mock<IQuestAssignmentService> questAssignmentServiceMock = new();
        questAssignmentServiceMock.Setup(q => q.ReplaceDailyUserQuestAsync(It.IsAny<UserQuest>(), It.IsAny<CancellationToken>()))
            .Callback<UserQuest, CancellationToken>((uq, ct) =>
            {
                // Simulate removal of old quest and addition of new one as the real service does
                ActDb.UserQuests.Remove(uq);
                ActDb.UserQuests.Add(newUserQuest);
            })
            .ReturnsAsync(newUserQuest);

        var handler = new RerollQuestCommand.Handler(
            ActDb,
            dateTimeMock.Object,
            activityLogServiceMock.Object,
            questAssignmentServiceMock.Object,
            Constants);

        var result = await handler.Handle(new RerollQuestCommand
        {
            UserId = user.Id,
            UserQuestId = userQuest.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        var dbUser = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
        Assert.That(dbUser.Gold, Is.EqualTo(700)); // 1000 - 300

        questAssignmentServiceMock.Verify(q => q.ReplaceDailyUserQuestAsync(It.IsAny<UserQuest>(), It.IsAny<CancellationToken>()), Times.Once);
        activityLogServiceMock.Verify(al => al.CreateQuestRerolledLog(
            user.Id, userQuest.Id, newUserQuest.Id, Constants.QuestRerollDailyQuestPrice), Times.Once);

        bool oldUserQuestExists = await AssertDb.UserQuests.AnyAsync(uq => uq.Id == userQuest.Id);
        Assert.That(oldUserQuestExists, Is.False); // should have been removed
    }
}

using Crpg.Application.Common;
using Crpg.Application.Quests.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Quests;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Quests.Services;

public class QuestAssignmentServiceTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        QuestDailyQuestsPerUser = 2,
        QuestWeeklyQuestsPerUser = 2,
    };

    [Test]
    public async Task AssignDailyQuestsToAllUsersShouldRemoveExpiredAndFillMissing()
    {
        User userWithCharacter = new() { Id = 1 };
        userWithCharacter.Characters.Add(new Character { Id = 10, UserId = 1, Name = "char" });

        User userWithoutCharacter = new() { Id = 2 };

        QuestDefinition dailyDefinition1 = new() { Id = 1, Type = QuestType.Daily, IsActive = true };
        QuestDefinition dailyDefinition2 = new() { Id = 2, Type = QuestType.Daily, IsActive = true };
        QuestDefinition dailyDefinition3 = new() { Id = 3, Type = QuestType.Daily, IsActive = true };

        ArrangeDb.Users.AddRange(userWithCharacter, userWithoutCharacter);
        ArrangeDb.QuestDefinitions.AddRange(dailyDefinition1, dailyDefinition2, dailyDefinition3);
        ArrangeDb.UserQuests.AddRange(
            new UserQuest
            {
                UserId = userWithCharacter.Id,
                QuestDefinitionId = dailyDefinition1.Id,
                QuestDefinition = dailyDefinition1,
                ExpiresAt = DateTime.UtcNow.Date.AddDays(2),
                IsRewardClaimed = false,
            },
            new UserQuest
            {
                UserId = userWithCharacter.Id,
                QuestDefinitionId = dailyDefinition2.Id,
                QuestDefinition = dailyDefinition2,
                ExpiresAt = DateTime.UtcNow.Date.AddDays(-1),
                IsRewardClaimed = false,
            });
        await ArrangeDb.SaveChangesAsync();

        QuestAssignmentService service = new(ActDb, Constants);

        await service.AssignDailyQuestsToAllUsersAsync();

        List<UserQuest> user1DailyQuests = await AssertDb.UserQuests
            .Include(uq => uq.QuestDefinition)
            .Where(uq => uq.UserId == userWithCharacter.Id && uq.QuestDefinition!.Type == QuestType.Daily)
            .ToListAsync();
        List<UserQuest> user2Quests = await AssertDb.UserQuests
            .Where(uq => uq.UserId == userWithoutCharacter.Id)
            .ToListAsync();

        Assert.That(user1DailyQuests.Count, Is.EqualTo(Constants.QuestDailyQuestsPerUser));
        Assert.That(user1DailyQuests.All(uq => uq.ExpiresAt > DateTime.UtcNow.Date), Is.True);
        Assert.That(user2Quests, Is.Empty);
    }

    [Test]
    public async Task AssignWeeklyQuestsToAllUsersShouldCreateCurrentAssignmentsAndAssignUsers()
    {
        User user1 = new() { Id = 1 };
        user1.Characters.Add(new Character { Id = 10, UserId = 1, Name = "char1" });
        User user2 = new() { Id = 2 };
        user2.Characters.Add(new Character { Id = 20, UserId = 2, Name = "char2" });
        User userWithoutCharacter = new() { Id = 3 };

        QuestDefinition weeklyDefinition1 = new() { Id = 1, Type = QuestType.Weekly, IsActive = true };
        QuestDefinition weeklyDefinition2 = new() { Id = 2, Type = QuestType.Weekly, IsActive = true };
        QuestDefinition weeklyDefinition3 = new() { Id = 3, Type = QuestType.Weekly, IsActive = true };

        ArrangeDb.Users.AddRange(user1, user2, userWithoutCharacter);
        ArrangeDb.QuestDefinitions.AddRange(weeklyDefinition1, weeklyDefinition2, weeklyDefinition3);
        await ArrangeDb.SaveChangesAsync();

        QuestAssignmentService service = new(ActDb, Constants);

        await service.AssignWeeklyQuestsToAllUsersAsync();

        List<WeeklyQuestAssignment> weeklyAssignments = await AssertDb.WeeklyQuestAssignments.ToListAsync();
        List<UserQuest> user1WeeklyQuests = await AssertDb.UserQuests
            .Include(uq => uq.QuestDefinition)
            .Where(uq => uq.UserId == user1.Id && uq.QuestDefinition!.Type == QuestType.Weekly)
            .ToListAsync();
        List<UserQuest> user2WeeklyQuests = await AssertDb.UserQuests
            .Include(uq => uq.QuestDefinition)
            .Where(uq => uq.UserId == user2.Id && uq.QuestDefinition!.Type == QuestType.Weekly)
            .ToListAsync();
        List<UserQuest> user3Quests = await AssertDb.UserQuests
            .Where(uq => uq.UserId == userWithoutCharacter.Id)
            .ToListAsync();

        Assert.That(weeklyAssignments.Count, Is.EqualTo(Constants.QuestWeeklyQuestsPerUser));
        Assert.That(user1WeeklyQuests.Count, Is.EqualTo(Constants.QuestWeeklyQuestsPerUser));
        Assert.That(user2WeeklyQuests.Count, Is.EqualTo(Constants.QuestWeeklyQuestsPerUser));
        Assert.That(user3Quests, Is.Empty);
    }

    [Test]
    public async Task AssignWeeklyQuestsToAllUsersShouldUseExistingAssignmentsAndAvoidDuplicates()
    {
        User user = new() { Id = 1 };
        user.Characters.Add(new Character { Id = 10, UserId = 1, Name = "char" });

        QuestDefinition weeklyDefinition1 = new() { Id = 1, Type = QuestType.Weekly, IsActive = true };
        QuestDefinition weeklyDefinition2 = new() { Id = 2, Type = QuestType.Weekly, IsActive = true };

        DateTime expiresAt = DateTime.UtcNow.AddDays(5);

        ArrangeDb.Users.Add(user);
        ArrangeDb.QuestDefinitions.AddRange(weeklyDefinition1, weeklyDefinition2);
        ArrangeDb.WeeklyQuestAssignments.AddRange(
            new WeeklyQuestAssignment { QuestDefinitionId = weeklyDefinition1.Id, AssignedAt = DateTime.UtcNow, ExpiresAt = expiresAt },
            new WeeklyQuestAssignment { QuestDefinitionId = weeklyDefinition2.Id, AssignedAt = DateTime.UtcNow, ExpiresAt = expiresAt });
        ArrangeDb.UserQuests.Add(new UserQuest
        {
            UserId = user.Id,
            QuestDefinitionId = weeklyDefinition1.Id,
            QuestDefinition = weeklyDefinition1,
            IsRewardClaimed = false,
            ExpiresAt = expiresAt,
        });
        await ArrangeDb.SaveChangesAsync();

        QuestAssignmentService service = new(ActDb, Constants);

        await service.AssignWeeklyQuestsToAllUsersAsync();

        List<WeeklyQuestAssignment> weeklyAssignments = await AssertDb.WeeklyQuestAssignments.ToListAsync();
        List<UserQuest> userWeeklyQuests = await AssertDb.UserQuests
            .Include(uq => uq.QuestDefinition)
            .Where(uq => uq.UserId == user.Id && uq.QuestDefinition!.Type == QuestType.Weekly)
            .ToListAsync();

        Assert.That(weeklyAssignments.Count, Is.EqualTo(2));
        Assert.That(userWeeklyQuests.Count, Is.EqualTo(2));

        var definitionIds = userWeeklyQuests.Select(uq => uq.QuestDefinitionId).ToHashSet();
        Assert.That(definitionIds.SetEquals([weeklyDefinition1.Id, weeklyDefinition2.Id]), Is.True);
    }

    [Test]
    public async Task AssignQuestsToNewUserShouldReplaceOldQuestsWithCurrentDailyAndWeekly()
    {
        User user = new() { Id = 1 };

        QuestDefinition oldDefinition = new() { Id = 1, Type = QuestType.Daily, IsActive = true };
        QuestDefinition dailyDefinition2 = new() { Id = 2, Type = QuestType.Daily, IsActive = true };
        QuestDefinition dailyDefinition3 = new() { Id = 3, Type = QuestType.Daily, IsActive = true };
        QuestDefinition weeklyDefinition = new() { Id = 4, Type = QuestType.Weekly, IsActive = true };

        ArrangeDb.Users.Add(user);
        ArrangeDb.QuestDefinitions.AddRange(oldDefinition, dailyDefinition2, dailyDefinition3, weeklyDefinition);
        ArrangeDb.WeeklyQuestAssignments.Add(new WeeklyQuestAssignment
        {
            QuestDefinitionId = weeklyDefinition.Id,
            AssignedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
        });
        ArrangeDb.UserQuests.Add(new UserQuest
        {
            Id = 100,
            UserId = user.Id,
            QuestDefinitionId = oldDefinition.Id,
            QuestDefinition = oldDefinition,
            IsRewardClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        });
        await ArrangeDb.SaveChangesAsync();

        QuestAssignmentService service = new(ActDb, Constants);

        await service.AssignQuestsToNewUserAsync(user.Id);

        List<UserQuest> userQuests = await AssertDb.UserQuests
            .Include(uq => uq.QuestDefinition)
            .Where(uq => uq.UserId == user.Id)
            .ToListAsync();

        Assert.That(userQuests.Count, Is.EqualTo(Constants.QuestDailyQuestsPerUser + 1));
        Assert.That(userQuests.Any(uq => uq.Id == 100), Is.False);
        Assert.That(userQuests.Count(uq => uq.QuestDefinition!.Type == QuestType.Daily), Is.EqualTo(Constants.QuestDailyQuestsPerUser));
        Assert.That(userQuests.Count(uq => uq.QuestDefinition!.Type == QuestType.Weekly), Is.EqualTo(1));
    }

    [Test]
    public async Task ReplaceDailyUserQuestShouldReplaceWithDifferentDefinitionAndKeepExpiry()
    {
        User user = new() { Id = 1 };

        QuestDefinition dailyDefinition1 = new() { Id = 1, Type = QuestType.Daily, IsActive = true };
        QuestDefinition dailyDefinition2 = new() { Id = 2, Type = QuestType.Daily, IsActive = true };

        DateTime expiresAt = DateTime.UtcNow.Date.AddDays(1);

        ArrangeDb.Users.Add(user);
        ArrangeDb.QuestDefinitions.AddRange(dailyDefinition1, dailyDefinition2);
        ArrangeDb.UserQuests.Add(new UserQuest
        {
            Id = 100,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = dailyDefinition1.Id,
            QuestDefinition = dailyDefinition1,
            IsRewardClaimed = true,
            ExpiresAt = expiresAt,
        });
        await ArrangeDb.SaveChangesAsync();

        QuestAssignmentService service = new(ActDb, Constants);

        UserQuest oldUserQuest = await ActDb.UserQuests
            .Include(uq => uq.User)
            .Include(uq => uq.QuestDefinition)
            .FirstAsync(uq => uq.Id == 100);

        UserQuest newUserQuest = await service.ReplaceDailyUserQuestAsync(oldUserQuest);

        bool oldQuestExists = await AssertDb.UserQuests.AnyAsync(uq => uq.Id == 100);
        UserQuest dbNewQuest = await AssertDb.UserQuests
            .Include(uq => uq.QuestDefinition)
            .FirstAsync(uq => uq.Id == newUserQuest.Id);

        Assert.That(oldQuestExists, Is.False);
        Assert.That(dbNewQuest.UserId, Is.EqualTo(user.Id));
        Assert.That(dbNewQuest.QuestDefinitionId, Is.Not.EqualTo(dailyDefinition1.Id));
        Assert.That(dbNewQuest.ExpiresAt, Is.EqualTo(expiresAt));
        Assert.That(dbNewQuest.IsRewardClaimed, Is.False);
    }
}

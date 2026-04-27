using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Quests;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Quests.Services;

public class QuestAssignmentService(ICrpgDbContext db, Constants constants) : IQuestAssignmentService
{

    public async Task AssignDailyQuestsToAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var oldUserQuests = await db.UserQuests.Where(uq => uq.ExpiresAt <= DateTime.UtcNow.Date).ToListAsync(cancellationToken);
        // ExecuteDelete can't be used because it is not supported by the in-memory provider which is used in our tests (https://github.com/dotnet/efcore/issues/30185).
        db.UserQuests.RemoveRange(oldUserQuests);

        var userIds = await db.Users
            .Where(u => u.Characters.Any())
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var availableDefinitions = await db.QuestDefinitions
            .Where(qd => qd.IsActive && qd.Type == QuestType.Daily)
            .ToListAsync(cancellationToken);

        var userActiveQuests = await db.UserQuests
            .Include(uq => uq.QuestDefinition)
            .Where(uq => uq.QuestDefinition!.Type == QuestType.Daily && uq.ExpiresAt > DateTime.UtcNow.Date).ToListAsync(cancellationToken);

        var userActiveQuestsCount = userActiveQuests
                    .GroupBy(x => x.UserId)
                    .ToDictionary(key => key.Key, value => value.Count());

        foreach (int userId in userIds)
        {
            int questsToAddCount = constants.QuestDailyQuestsPerUser - userActiveQuestsCount.GetValueOrDefault(userId);

            if (questsToAddCount <= 0)
            {
                continue;
            }

            var selectedQuests = availableDefinitions.Shuffle().Take(questsToAddCount);

            foreach (var definition in selectedQuests)
            {
                var userQuest = new UserQuest
                {
                    UserId = userId,
                    QuestDefinitionId = definition.Id,
                    IsRewardClaimed = false,
                    ExpiresAt = DateTime.UtcNow.Date.AddDays(1),
                };
                db.UserQuests.Add(userQuest);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignWeeklyQuestsToAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var oldUserQuests = await db.UserQuests.Where(uq => uq.ExpiresAt <= DateTime.UtcNow.Date).ToListAsync(cancellationToken);
        db.UserQuests.RemoveRange(oldUserQuests);

        var oldWeeklyAssignments = await db.WeeklyQuestAssignments.Where(wqa => wqa.ExpiresAt <= DateTime.UtcNow.Date).ToListAsync(cancellationToken);
        db.WeeklyQuestAssignments.RemoveRange(oldWeeklyAssignments);

        var userIds = await db.Users
            .Where(u => u.Characters.Any())
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var availableWeeklyDefinitions = await db.QuestDefinitions
            .Where(qd => qd.IsActive && qd.Type == QuestType.Weekly)
            .ToListAsync(cancellationToken);

        if (availableWeeklyDefinitions.Count == 0)
        {
            return;
        }

        var currentWeeklyAssignments = await db.WeeklyQuestAssignments
            .Where(wqa => wqa.ExpiresAt > DateTime.UtcNow)
            .Select(wqa => wqa.QuestDefinitionId)
            .ToListAsync(cancellationToken);

        List<int> selectedWeeklyQuestIds;
        var now = DateTime.UtcNow;
        var expiresAt = NextMonday(now.Date);
        if (currentWeeklyAssignments.Count == 0)
        {
            // Create new weekly assignments for this week
            selectedWeeklyQuestIds = availableWeeklyDefinitions
                .Shuffle()
                .Take(constants.QuestWeeklyQuestsPerUser)
                .Select(q => q.Id)
                .ToList();

            foreach (int questId in selectedWeeklyQuestIds)
            {
                var assignment = new WeeklyQuestAssignment
                {
                    QuestDefinitionId = questId,
                    AssignedAt = now,
                    ExpiresAt = expiresAt,
                };
                db.WeeklyQuestAssignments.Add(assignment);
            }
        }
        else
        {
            // Use existing assignments for this week
            selectedWeeklyQuestIds = currentWeeklyAssignments;
        }

        // Get existing weekly quests per user to avoid duplicates
        var userWeeklyQuestsList = await db.UserQuests
            .Include(uq => uq.QuestDefinition)
            .Where(uq => uq.QuestDefinition!.Type == QuestType.Weekly)
            .ToListAsync(cancellationToken);

        var userWeeklyQuests = userWeeklyQuestsList
                    .GroupBy(x => x.UserId)
                    .ToDictionary(g => g.Key, g => g.Select(uq => uq.QuestDefinitionId).ToHashSet());

        foreach (int userId in userIds)
        {
            var existingWeeklyQuestIds = userWeeklyQuests.GetValueOrDefault(userId, new HashSet<int>());
            int questsToAddCount = constants.QuestWeeklyQuestsPerUser - existingWeeklyQuestIds.Count;

            if (questsToAddCount <= 0)
            {
                continue;
            }

            // Take quests that the user doesn't already have from the selected weekly quests
            var questsToAssign = selectedWeeklyQuestIds
                .Where(q => !existingWeeklyQuestIds.Contains(q))
                .Take(questsToAddCount);

            foreach (int questDefinitionId in questsToAssign)
            {
                var userQuest = new UserQuest
                {
                    UserId = userId,
                    QuestDefinitionId = questDefinitionId,
                    IsRewardClaimed = false,
                    ExpiresAt = expiresAt,
                };
                db.UserQuests.Add(userQuest);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignQuestsToNewUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        // Daily
        var oldUserQuests = await db.UserQuests.Where(uq => uq.UserId == userId).ToListAsync(cancellationToken);
        db.UserQuests.RemoveRange(oldUserQuests);

        var availableDailyDefinitions = await db.QuestDefinitions
            .Where(qd => qd.IsActive && qd.Type == QuestType.Daily)
            .ToListAsync(cancellationToken);

        var selectedDailyQuests = availableDailyDefinitions.Shuffle().Take(constants.QuestDailyQuestsPerUser);

        foreach (var definition in selectedDailyQuests)
        {
            var userQuest = new UserQuest
            {
                UserId = userId,
                QuestDefinitionId = definition.Id,
                IsRewardClaimed = false,
                ExpiresAt = DateTime.UtcNow.Date.AddDays(1),
            };
            db.UserQuests.Add(userQuest);
        }

        // Weekly
        var currentWeeklyAssignments = await db.WeeklyQuestAssignments
            .Where(wqa => wqa.ExpiresAt > DateTime.UtcNow)
            .Select(wqa => wqa.QuestDefinitionId)
            .Take(constants.QuestWeeklyQuestsPerUser)
            .ToListAsync(cancellationToken);

        if (currentWeeklyAssignments.Count != 0)
        {
            foreach (int questDefinitionId in currentWeeklyAssignments)
            {
                var userQuest = new UserQuest
                {
                    UserId = userId,
                    QuestDefinitionId = questDefinitionId,
                    IsRewardClaimed = false,
                    ExpiresAt = NextMonday(DateTime.UtcNow.Date),
                };
                db.UserQuests.Add(userQuest);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserQuest> ReplaceDailyUserQuestAsync(UserQuest userQuest,
        CancellationToken cancellationToken = default)
    {
        var questDefinitions = await db.QuestDefinitions
            .Where(qd => qd.IsActive && qd.Type == QuestType.Daily && userQuest.QuestDefinition!.Id != qd.Id)
            .ToListAsync(cancellationToken);

        var randomQuestDefinition = questDefinitions.Shuffle().FirstOrDefault() ?? throw new InvalidOperationException("No available daily quest definitions found.");

        var newUserQuest = new UserQuest
        {
            User = userQuest.User,
            QuestDefinition = randomQuestDefinition,
            IsRewardClaimed = false,
            ExpiresAt = userQuest.ExpiresAt,
        };

        db.UserQuests.Remove(userQuest);
        db.UserQuests.Add(newUserQuest);
        await db.SaveChangesAsync(cancellationToken);

        return newUserQuest;
    }

    private static DateTime NextMonday(DateTime date)
    {
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(daysUntilMonday == 0 ? 7 : daysUntilMonday);
    }
}

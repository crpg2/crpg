using Crpg.Domain.Entities.Quests;

namespace Crpg.Application.Quests.Services;

/// <summary>
/// Service for assigning quests to users.
/// </summary>
public interface IQuestAssignmentService
{
    /// <summary>
    /// Assigns daily quests to all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AssignDailyQuestsToAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns weekly quests to all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AssignWeeklyQuestsToAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Assign quests to new users.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AssignQuestsToNewUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reroll selected quest.
    /// </summary>
    /// <param name="userQuest">Old user quest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<UserQuest> ReplaceDailyUserQuestAsync(UserQuest userQuest, CancellationToken cancellationToken = default);
}

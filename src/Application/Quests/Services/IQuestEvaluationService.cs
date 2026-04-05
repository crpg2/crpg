using Crpg.Domain.Entities.Quests;

namespace Crpg.Application.Quests.Services;

public interface IQuestEvaluationService
{
    Task<Dictionary<int, int>> ComputeCurrentValuesAsync(List<UserQuest> userQuests, CancellationToken cancellationToken = default);
}

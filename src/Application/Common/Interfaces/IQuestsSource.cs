using Crpg.Domain.Entities.Quests;

namespace Crpg.Application.Common.Interfaces;

public interface IQuestsSource
{
    Task<IEnumerable<QuestDefinition>> LoadQuests();
}

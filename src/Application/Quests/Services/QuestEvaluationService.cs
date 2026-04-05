using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.GameEvents;
using Crpg.Domain.Entities.Quests;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Quests.Services;

public class QuestEvaluationService(ICrpgDbContext db) : IQuestEvaluationService
{
    public async Task<Dictionary<int, int>> ComputeCurrentValuesAsync(List<UserQuest> userQuests,
        CancellationToken cancellationToken = default)
    {
        if (userQuests.Count == 0)
        {
            return [];
        }

        var results = new Dictionary<int, int>();

        HashSet<int> userIds = userQuests.Select(uq => uq.UserId).ToHashSet();

        var eventTypes = userQuests.Select(q => q.QuestDefinition!.EventType).Distinct().ToList();
        var earliestDate = userQuests.Min(q => q.CreatedAt.Date);

        var events = await db.GameEvents
            .Where(ge => userIds.Contains(ge.UserId!.Value)
                         && eventTypes.Contains(ge.Type)
                         && ge.CreatedAt >= earliestDate)
            .ToListAsync(cancellationToken);

        foreach (var userQuest in userQuests)
        {
            var questDefinition = userQuest.QuestDefinition!;
            var questEvents = events
                .Where(ge => ge.Type == questDefinition.EventType
                             && ge.CreatedAt >= userQuest.CreatedAt.Date)
                .ToList();

            // Apply event filters in memory if any
            if (questDefinition.EventFiltersJson != null && questDefinition.EventFiltersJson.Length > 0)
            {
                questEvents = questEvents.Where(ge => ge.EventData != null
                                                      && questDefinition.EventFiltersJson.Any(filter =>
                                                          filter.All(kvp =>
                                                          {
                                                              if (!Enum.TryParse<GameEventField>(kvp.Key,
                                                                      out var field))
                                                              {
                                                                  return false;
                                                              }

                                                              return ge.EventData!.TryGetValue(field,
                                                                         out string? value) &&
                                                                     value == kvp.Value;
                                                          }))).ToList();
            }

            int value = questDefinition.AggregationType switch
            {
                QuestAggregationType.Count => questEvents.Count,
                QuestAggregationType.Sum when questDefinition.AggregationField != null =>
                    questEvents.Sum(ev =>
                    {
                        if (ev.EventData != null &&
                            ev.EventData.TryGetValue(questDefinition.AggregationField.Value, out string? strValue) &&
                            int.TryParse(strValue, out int intValue))
                        {
                            return intValue;
                        }

                        return 0;
                    }),
                _ => 0,
            };

            results[userQuest.Id] = value;
        }

        return results;
    }
}

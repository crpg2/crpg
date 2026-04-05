using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Quests;

namespace Crpg.Application.Quests.Models;

public record UserQuestViewModel : IMapFrom<UserQuest>
{
    public int Id { get; init; }
    public bool IsRewardClaimed { get; init; }
    public DateTime ExpiresAt { get; init; }
    public int CurrentValue { get; init; }

    public QuestDefinitionViewModel QuestDefinition { get; init; } = default!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserQuest, UserQuestViewModel>()
            .ForMember(dest => dest.CurrentValue, opt => opt.Ignore());
    }
}

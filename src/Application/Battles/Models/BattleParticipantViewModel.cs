using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record BattleParticipantViewModel : IMapFrom<BattleParticipant>
{
    public int Id { get; init; }
    public UserPublicViewModel User { get; init; } = default!;
    public CharacterPublicViewModel Character { get; init; } = default!;
    public BattleSide Side { get; init; }
    public BattleParticipantType Type { get; set; }
    [JsonRequired]
    public int? MercenaryApplicationId { get; set; }

    [JsonRequired]
    public BattleParticipantStatisticViewModel? Statistic { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<BattleParticipant, BattleParticipantViewModel>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.Character!.User));
    }
}

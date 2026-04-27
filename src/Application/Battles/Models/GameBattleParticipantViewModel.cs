using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record GameBattleParticipantViewModel : IMapFrom<BattleParticipant>
{
    public int Id { get; init; }
    public BattleSide Side { get; init; }
    public BattleParticipantType Type { get; init; }
    public GameUserViewModel User { get; init; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<BattleParticipant, GameBattleParticipantViewModel>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.Character!.User));
    }
}

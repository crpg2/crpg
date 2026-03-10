using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Battles;

namespace Crpg.Application.Battles.Models;

public record BattleMercenaryApplicationViewModel : IMapFrom<BattleMercenaryApplication>
{
    public int Id { get; init; }
    public DateTime CreatedAt { get; set; }
    public UserPublicViewModel User { get; init; } = default!;
    public CharacterPublicViewModel Character { get; init; } = default!;
    public BattleSide Side { get; init; }
    public int Wage { get; init; }
    public string Note { get; init; } = string.Empty;
    public BattleMercenaryApplicationStatus Status { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<BattleMercenaryApplication, BattleMercenaryApplicationViewModel>()
            .ForMember(dest => dest.Character, opt => opt.MapFrom(src => src.Character))
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.Character!.User));
    }
}

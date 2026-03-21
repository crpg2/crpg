using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Games.Models;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Parties;

namespace Crpg.Application.Parties.Models;

public record BattlePartyViewModel : IMapFrom<Party>
{
    public int Id { get; init; }
    public int Troops { get; init; }
    public GameUserViewModel User { get; init; } = default!;
    public IList<ItemIdStackViewModel> Items { get; init; } = [];

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Party, BattlePartyViewModel>()
            .ForMember(dest => dest.Troops, opt => opt.MapFrom(src => (int)src.Troops));
    }
}

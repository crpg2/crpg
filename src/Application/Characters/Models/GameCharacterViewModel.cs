using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models;

public record GameCharacterViewModel : IMapFrom<Character>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Generation { get; init; }
    public int Level { get; init; }
    public int Experience { get; init; }
    public CharacterClass Class { get; init; }
    public bool ForTournament { get; init; }
    public CharacterCharacteristicsViewModel Characteristics { get; init; } = new();

    public CharacterStatisticsViewModel Statistics { get; set; } = new();
    public IList<GameEquippedItemViewModel> EquippedItems { get; init; } = Array.Empty<GameEquippedItemViewModel>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Character, GameCharacterViewModel>()
            .ForMember(gc => gc.Statistics, opt => opt.MapFrom(c => c.Statistics.FirstOrDefault()));
    }
}

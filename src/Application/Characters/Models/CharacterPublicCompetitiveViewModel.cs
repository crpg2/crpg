using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models;

public record CharacterPublicCompetitiveViewModel : IMapFrom<Character>
{
    public int Id { get; init; }
    public int Level { get; init; }
    public CharacterClass Class { get; init; }
    public IList<CharacterStatisticsViewModel> Statistics { get; init; } = Array.Empty<CharacterStatisticsViewModel>();
    public UserPublicViewModel User { get; init; } = new();
}

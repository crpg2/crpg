using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models;

public record CharacterPublicViewModel : IMapFrom<Character>
{
    public int Id { get; init; }
    public int Level { get; init; }
    public CharacterClass Class { get; init; }
}

using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Parties;

namespace Crpg.Application.Parties.Models;

/// <summary>
/// View of a <see cref="Party"/> when seen outside of the campaign map (e.g. battle info).
/// </summary>
public record PartyPublicViewModel : IMapFrom<Party>
{
    public int Id { get; init; }
    public UserPublicViewModel User { get; init; } = default!;
}

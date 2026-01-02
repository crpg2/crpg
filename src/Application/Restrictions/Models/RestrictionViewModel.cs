using System.Text.Json.Serialization;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Restrictions;

namespace Crpg.Application.Restrictions.Models;

public record RestrictionViewModel : IMapFrom<Restriction>
{
    public int Id { get; init; }
    [JsonRequired]
    public UserPrivateViewModel RestrictedUser { get; init; } = default!;
    public TimeSpan Duration { get; init; }
    public RestrictionType Type { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string PublicReason { get; set; } = string.Empty;
    [JsonRequired]
    public UserPublicViewModel RestrictedByUser { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}

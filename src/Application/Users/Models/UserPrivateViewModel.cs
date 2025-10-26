using System.Text.Json.Serialization;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Users.Models;

public record UserPrivateViewModel : IMapFrom<User>
{
    public int Id { get; init; }
    public Platform Platform { get; init; }
    public int Gold { get; set; }
    public int HeirloomPoints { get; set; }
    public float ExperienceMultiplier { get; set; }
    public string PlatformUserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    [JsonRequired]
    public Uri? Avatar { get; init; }
    public Region Region { get; init; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDonor { get; set; }
    public string Note { get; init; } = string.Empty;
    [JsonRequired]
    public int? ActiveCharacterId { get; init; }
    [JsonRequired]
    public UserClanViewModel? ClanMembership { get; init; }
}

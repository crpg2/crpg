using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Notifications;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Users.Models;

public record UserViewModel : IMapFrom<User>
{
    public int Id { get; init; }
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Gold { get; init; }
    public int HeirloomPoints { get; init; }
    public float ExperienceMultiplier { get; init; }
    public Role Role { get; init; }
    public Region Region { get; init; }
    public bool IsDonor { get; init; }

    [JsonRequired]
    public Uri? Avatar { get; init; }

    [JsonRequired]
    public int? ActiveCharacterId { get; init; }
    public int UnreadNotificationsCount { get; init; }
    public UserClanViewModel? ClanMembership { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserViewModel>()
            .ForMember(u => u.UnreadNotificationsCount, opt => opt.MapFrom(u => u.Notifications.Where(un => un.State == NotificationState.Unread).Count()));
    }
}

using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Notifications;

namespace Crpg.Application.Notifications.Models;

public record UserNotificationViewModel : IMapFrom<UserNotification>
{
    public int Id { get; init; }
    public NotificationState State { get; init; }
    public NotificationType Type { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();

    public DateTime CreatedAt { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserNotification, UserNotificationViewModel>()
            .ForMember(l => l.Metadata, opt => opt.MapFrom(l =>
                l.Metadata.ToDictionary(m => m.Key, m => m.Value)));
    }
}

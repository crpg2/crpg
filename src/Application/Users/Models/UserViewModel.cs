using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Marketplace;
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
    public int ReservedGold { get; init; }
    public int HeirloomPoints { get; init; }
    public int ReservedHeirloomPoints { get; init; }
    public float ExperienceMultiplier { get; init; }
    public Role Role { get; init; }
    public Region Region { get; init; }
    public bool IsDonor { get; init; }
    public bool IsRecent { get; set; }
    [JsonRequired]
    public Uri? Avatar { get; init; }
    [JsonRequired]
    public int? ActiveCharacterId { get; init; }
    public int UnreadNotificationsCount { get; init; }
    public int ActiveMarketplaceListingsCount { get; init; }
    [JsonRequired]
    public UserClanViewModel? ClanMembership { get; init; }

    public void Mapping(Profile profile) => profile.CreateMap<User, UserViewModel>()
            .ForMember(u => u.UnreadNotificationsCount,
                opt => opt.MapFrom(u => u.Notifications.Count(un => un.State == NotificationState.Unread)))
            .ForMember(u => u.ReservedGold,
                opt => opt.MapFrom(u =>
                    u.MarketplaceListings.Sum(l =>
                        l.GoldFee +
                        l.Assets.Where(a => a.Side == MarketplaceListingAssetSide.Offered).Sum(a => a.Gold))))
            .ForMember(u => u.ReservedHeirloomPoints,
                opt => opt.MapFrom(u =>
                    u.MarketplaceListings.SelectMany(l =>
                        l.Assets.Where(a => a.Side == MarketplaceListingAssetSide.Offered)).Sum(a => a.HeirloomPoints)))
            .ForMember(u => u.ActiveMarketplaceListingsCount, opt => opt.MapFrom(u => u.MarketplaceListings.Count))
            .ForMember(dest => dest.IsRecent, opt => opt.Ignore());
}

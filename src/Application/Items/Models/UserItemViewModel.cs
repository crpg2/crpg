using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Marketplace;

namespace Crpg.Application.Items.Models;

public record UserItemViewModel : IMapFrom<UserItem>
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public ItemViewModel Item { get; init; } = default!;
    public bool IsListedOnMarketplace { get; init; }
    public bool IsBroken { get; init; }
    public DateTime CreatedAt { get; init; }
    [JsonRequired]
    public ClanMemberViewModel? ClanArmoryLender { get; init; }
    public bool IsPersonal { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserItem, UserItemViewModel>()
            .ForMember(ui => ui.ClanArmoryLender, config => config.MapFrom(ui => ui.ClanArmoryItem != null ? ui.ClanArmoryItem!.Lender : null))
            .ForMember(ui => ui.IsPersonal, config => config.MapFrom(ui => ui.PersonalItem != null))
            .ForMember(ui => ui.IsListedOnMarketplace, config => config.MapFrom(ui => ui.MarketplaceOfferAssets.Any(a => a.Side == MarketplaceOfferAssetSide.Offered)));
    }
}

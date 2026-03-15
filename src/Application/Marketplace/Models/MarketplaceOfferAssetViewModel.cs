using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Marketplace;

namespace Crpg.Application.Marketplace.Models;

public record MarketplaceOfferAssetViewModel : IMapFrom<MarketplaceOfferAsset>
{
    public MarketplaceOfferAssetSide Side { get; init; }
    public int Gold { get; init; }
    public int HeirloomPoints { get; init; }

    [JsonRequired]
    public ItemSummaryViewModel? Item { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<MarketplaceOfferAsset, MarketplaceOfferAssetViewModel>()
            .ForMember(d => d.Item, opt => opt.MapFrom(s => s.UserItem != null ? s.UserItem.Item : s.Item));
    }
}

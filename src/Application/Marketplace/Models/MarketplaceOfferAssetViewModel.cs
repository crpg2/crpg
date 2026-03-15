using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Marketplace;

namespace Crpg.Application.Marketplace.Models;

public record MarketplaceOfferAssetViewModel : IMapFrom<MarketplaceOfferAsset>
{
    public int Id { get; init; }
    public MarketplaceOfferAssetSide Side { get; init; }
    public MarketplaceAssetType Type { get; init; }
    public int Amount { get; init; }
    public string? ItemId { get; init; }
    public int? UserItemId { get; init; }
    public ItemViewModel? Item { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<MarketplaceOfferAsset, MarketplaceOfferAssetViewModel>()
            .ForMember(vm => vm.Item, cfg => cfg.MapFrom(src => src.UserItem != null ? src.UserItem.Item : null));
    }
}

using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Marketplace;

namespace Crpg.Application.Marketplace.Models;

public record MarketplaceOfferViewModel : IMapFrom<MarketplaceOffer>
{
    public int Id { get; init; }
    public UserPublicViewModel Seller { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
    public MarketplaceOfferAssetViewModel Offer { get; init; } = default!;
    public MarketplaceOfferAssetViewModel Request { get; init; } = default!;
    public int GoldFee { get; init; }

    public void Mapping(Profile profile) => profile
        .CreateMap<MarketplaceOffer, MarketplaceOfferViewModel>()
        .ForMember(d => d.Offer, opt => opt.MapFrom(s => s.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Offered)))
        .ForMember(d => d.Request, opt => opt.MapFrom(s => s.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Requested)));
}

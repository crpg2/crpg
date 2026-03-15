namespace Crpg.Application.Marketplace.Models;

public record MarketplaceOffersPageViewModel
{
    public IList<MarketplaceOfferViewModel> Items { get; init; } = [];
    public int TotalCount { get; init; }
}

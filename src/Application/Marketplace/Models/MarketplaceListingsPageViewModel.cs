namespace Crpg.Application.Marketplace.Models;

public record MarketplaceListingsPageViewModel
{
    public IList<MarketplaceListingViewModel> Items { get; init; } = [];
    public int TotalCount { get; init; }
}

namespace Crpg.Application.Marketplace.Models;

public record MarketplaceListingsHistoryPageViewModel
{
    public IList<MarketplaceListingHistoryViewModel> Items { get; init; } = [];
    public int TotalCount { get; init; }
}

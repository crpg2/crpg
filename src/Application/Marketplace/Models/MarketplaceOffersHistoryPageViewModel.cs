namespace Crpg.Application.Marketplace.Models;

public record MarketplaceOffersHistoryPageViewModel
{
    public IList<MarketplaceOfferHistoryViewModel> Items { get; init; } = [];
    public int TotalCount { get; init; }
}

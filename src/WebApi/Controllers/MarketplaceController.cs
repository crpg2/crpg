using System.Net;
using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Commands;
using Crpg.Application.Marketplace.Models;
using Crpg.Application.Marketplace.Queries;
using Crpg.Domain.Entities.Items;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class MarketplaceController : BaseController
{
    [HttpGet("listings")]
    public Task<ActionResult<Result<MarketplaceListingsPageViewModel>>> GetListings(
        [FromQuery] string? offeredItemId = null,
        [FromQuery] IList<int>? offeredItemRanks = null,
        [FromQuery] ItemType? offeredItemType = null,
        [FromQuery] string? offeredGold = null,
        [FromQuery] string? offeredHeirloomPoints = null,
        [FromQuery] string? requestedItemId = null,
        [FromQuery] IList<int>? requestedItemRanks = null,
        [FromQuery] ItemType? requestedItemType = null,
        [FromQuery] string? requestedGold = null,
        [FromQuery] string? requestedHeirloomPoints = null,
        [FromQuery] int? sellerId = null,
        [FromQuery] bool onlyAffordable = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        return ResultToActionAsync(Mediator.Send(new GetMarketplaceListingsQuery
        {
            Offered = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemId = offeredItemId,
                ItemRanks = offeredItemRanks,
                ItemType = offeredItemType,
                Gold = ParseCurrencyFilter(offeredGold),
                HeirloomPoints = ParseCurrencyFilter(offeredHeirloomPoints),
            },
            Requested = new GetMarketplaceListingsQuery.SideFilter
            {
                ItemId = requestedItemId,
                ItemRanks = requestedItemRanks,
                ItemType = requestedItemType,
                Gold = ParseCurrencyFilter(requestedGold),
                HeirloomPoints = ParseCurrencyFilter(requestedHeirloomPoints),
            },
            SellerId = sellerId,
            BuyerId = onlyAffordable ? CurrentUser.User!.Id : null,
            Page = page,
            PageSize = pageSize,
        }));
    }

    [HttpPost("listings")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public Task<ActionResult<Result<MarketplaceListingViewModel>>> CreateListing([FromBody] CreateMarketplaceListingCommand req)
    {
        req.UserId = CurrentUser.User!.Id;
        return ResultToCreatedAtActionAsync(nameof(GetListings), null, null, Mediator.Send(req));
    }

    [HttpDelete("listings/{listingId:int}")]
    public Task<ActionResult> CancelListing([FromRoute] int listingId)
    {
        return ResultToActionAsync(Mediator.Send(new CancelMarketplaceListingCommand
        {
            UserId = CurrentUser.User!.Id,
            ListingId = listingId,
        }));
    }

    [HttpGet("listings/history")]
    public Task<ActionResult<Result<MarketplaceListingsHistoryPageViewModel>>> GetListingsHistory(
        [FromQuery] int? buyerId = null,
        [FromQuery] int? sellerId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        return ResultToActionAsync(Mediator.Send(new GetMarketplaceListingsHistoryQuery
        {
            BuyerId = buyerId,
            SellerId = sellerId,
            Page = page,
            PageSize = pageSize,
        }));
    }

    [HttpPost("listings/{listingId:int}/accept")]
    public Task<ActionResult> AcceptOffer([FromRoute] int listingId)
    {
        return ResultToActionAsync(Mediator.Send(new AcceptMarketplaceListingCommand
        {
            UserId = CurrentUser.User!.Id,
            ListingId = listingId,
        }));
    }

    private static MarketplaceCurrencyFilter ParseCurrencyFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter) || string.Equals(filter, "any", StringComparison.OrdinalIgnoreCase))
        {
            return new()
            {
                Mode = MarketplaceCurrencyFilterMode.Any,
            };
        }

        if (string.Equals(filter, "none", StringComparison.OrdinalIgnoreCase))
        {
            return new()
            {
                Mode = MarketplaceCurrencyFilterMode.None,
            };
        }

        (int? min, int? max)? range = ParseRange(filter);
        if (range.HasValue)
        {
            return new()
            {
                Mode = MarketplaceCurrencyFilterMode.CustomRange,
                Range = range,
            };
        }

        return new()
        {
            Mode = MarketplaceCurrencyFilterMode.Any,
        };
    }

    private static (int? min, int? max)? ParseRange(string? range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            return null;
        }

        string[] parts = range.Split(',', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return null;
        }

        bool minOk = string.IsNullOrWhiteSpace(parts[0]) || int.TryParse(parts[0], out _);
        bool maxOk = string.IsNullOrWhiteSpace(parts[1]) || int.TryParse(parts[1], out _);
        if (!minOk || !maxOk)
        {
            return null;
        }

        int? min = string.IsNullOrWhiteSpace(parts[0]) ? null : int.Parse(parts[0]);
        int? max = string.IsNullOrWhiteSpace(parts[1]) ? null : int.Parse(parts[1]);
        return (min, max);
    }
}

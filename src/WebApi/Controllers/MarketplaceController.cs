using System.Net;
using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Commands;
using Crpg.Application.Marketplace.Models;
using Crpg.Application.Marketplace.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crpg.WebApi.Controllers;

[Authorize(Policy = UserPolicy)]
public class MarketplaceController : BaseController
{
    [HttpGet("offers")]
    public Task<ActionResult<Result<IList<MarketplaceOfferViewModel>>>> GetOffers([FromQuery] string? requestedItemId)
    {
        return ResultToActionAsync(Mediator.Send(new GetMarketplaceOffersQuery
        {
            RequestedItemId = requestedItemId,
        }));
    }

    [HttpGet("offers/self")]
    public Task<ActionResult<Result<IList<MarketplaceOfferViewModel>>>> GetSelfOffers()
    {
        return ResultToActionAsync(Mediator.Send(new GetSelfMarketplaceOffersQuery
        {
            UserId = CurrentUser.User!.Id,
        }));
    }

    [HttpPost("offers")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    public Task<ActionResult<Result<MarketplaceOfferViewModel>>> CreateOffer([FromBody] CreateMarketplaceOfferCommand req)
    {
        req.UserId = CurrentUser.User!.Id;
        return ResultToCreatedAtActionAsync(nameof(GetSelfOffers), null, null, Mediator.Send(req));
    }

    [HttpDelete("offers/{offerId:int}")]
    public Task<ActionResult<Result<MarketplaceOfferViewModel>>> CancelOffer([FromRoute] int offerId)
    {
        return ResultToActionAsync(Mediator.Send(new CancelMarketplaceOfferCommand
        {
            UserId = CurrentUser.User!.Id,
            OfferId = offerId,
        }));
    }

    [HttpPost("offers/{offerId:int}/accept")]
    public Task<ActionResult<Result<MarketplaceOfferViewModel>>> AcceptOffer([FromRoute] int offerId)
    {
        return ResultToActionAsync(Mediator.Send(new AcceptMarketplaceOfferCommand
        {
            UserId = CurrentUser.User!.Id,
            OfferId = offerId,
        }));
    }
}

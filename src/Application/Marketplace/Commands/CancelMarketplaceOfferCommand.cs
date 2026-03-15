using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Marketplace.Models;
using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Marketplace.Commands;

public record CancelMarketplaceOfferCommand : IMediatorRequest<MarketplaceOfferViewModel>
{
    [JsonIgnore]
    public int UserId { get; set; }

    [JsonIgnore]
    public int OfferId { get; set; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<CancelMarketplaceOfferCommand, MarketplaceOfferViewModel>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<MarketplaceOfferViewModel>> Handle(CancelMarketplaceOfferCommand req, CancellationToken cancellationToken)
        {
            var offer = await _db.MarketplaceOffers
                .Include(o => o.Assets)
                .FirstOrDefaultAsync(o => o.Id == req.OfferId, cancellationToken);
            if (offer == null)
            {
                return new(CommonErrors.MarketplaceOfferNotFound(req.OfferId));
            }

            if (offer.SellerUserId != req.UserId)
            {
                return new(CommonErrors.MarketplaceOfferNotAllowed(req.OfferId));
            }

            var seller = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (seller == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            DateTime now = DateTime.UtcNow;
            MarketplaceOfferLifecycle.ExpireOfferWithRefund(seller, offer, now);
            if (offer.Status == MarketplaceOfferStatus.Expired)
            {
                await _db.SaveChangesAsync(cancellationToken);
                return new(CommonErrors.MarketplaceOfferExpired(req.OfferId));
            }

            if (offer.Status != MarketplaceOfferStatus.Active)
            {
                return new(CommonErrors.MarketplaceOfferInvalidStatus(req.OfferId, offer.Status));
            }

            foreach (var asset in offer.Assets.Where(a => a.Side == MarketplaceOfferAssetSide.Offered))
            {
                switch (asset.Type)
                {
                    case MarketplaceAssetType.Gold:
                        seller.Gold += asset.Amount;
                        break;
                    case MarketplaceAssetType.HeirloomPoints:
                        seller.HeirloomPoints += asset.Amount;
                        break;
                }
            }

            offer.Status = MarketplaceOfferStatus.Cancelled;
            offer.ClosedAt = now;

            await _db.SaveChangesAsync(cancellationToken);

            return new(_mapper.Map<MarketplaceOfferViewModel>(offer));
        }
    }
}

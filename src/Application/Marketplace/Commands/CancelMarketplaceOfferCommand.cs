using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Marketplace.Commands;

public record CancelMarketplaceOfferCommand : IMediatorRequest
{
    [JsonIgnore]
    public int UserId { get; set; }

    [JsonIgnore]
    public int OfferId { get; set; }

    internal class Handler(ICrpgDbContext db, IActivityLogService activityLogService, IMarketplaceService marketplaceService) : IMediatorRequestHandler<CancelMarketplaceOfferCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CancelMarketplaceOfferCommand>();
        private readonly ICrpgDbContext _db = db;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IMarketplaceService _marketplaceService = marketplaceService;

        public async ValueTask<Result> Handle(CancelMarketplaceOfferCommand req, CancellationToken cancellationToken)
        {
            var offer = await _db.MarketplaceOffers
                .AsSplitQuery()
                .Include(o => o.Assets)
                    .ThenInclude(a => a.UserItem)
                        .ThenInclude(ui => ui!.Item)
                .Include(o => o.Assets)
                    .ThenInclude(a => a!.Item)
                .Include(o => o.Seller)
                .FirstOrDefaultAsync(o => o.Id == req.OfferId, cancellationToken);
            if (offer == null)
            {
                return new(CommonErrors.MarketplaceOfferNotFound(req.OfferId));
            }

            if (offer.Seller == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (offer.Seller.Id != req.UserId)
            {
                return new(CommonErrors.MarketplaceOfferNotAllowed(req.OfferId));
            }

            _marketplaceService.RefundAndRemoveOffer(_db, offer);

            _db.ActivityLogs.Add(_activityLogService.CreateMarketplaceOfferCancelledLog(userId: req.UserId, offerId: offer.Id, goldFee: offer.GoldFee, offered: offer.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Offered)!, requested: offer.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Requested)!));

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Marketplace offer '{0}' cancelled by user '{1}'", offer.Id, req.UserId);
            return Result.NoErrors;
        }
    }
}

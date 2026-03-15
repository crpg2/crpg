using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.Marketplace;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Marketplace.Commands;

public record DeleteExpiredMarketplaceOffersCommand : IMediatorRequest
{
    internal class Handler(ICrpgDbContext db, IDateTime dateTime, IUserNotificationService userNotificationService, IActivityLogService activityLogService, IMarketplaceService marketplaceService) : IMediatorRequestHandler<DeleteExpiredMarketplaceOffersCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteExpiredMarketplaceOffersCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IDateTime _dateTime = dateTime;
        private readonly IUserNotificationService _userNotificationService = userNotificationService;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IMarketplaceService _marketplaceService = marketplaceService;

        public async ValueTask<Result> Handle(DeleteExpiredMarketplaceOffersCommand req, CancellationToken cancellationToken)
        {
            DateTime now = _dateTime.UtcNow;

            MarketplaceOffer[] expiredOffers = await _db.MarketplaceOffers
                .AsSplitQuery()
                .Include(o => o.Assets)
                    .ThenInclude(a => a.UserItem)
                        .ThenInclude(ui => ui!.Item)
                .Include(o => o.Assets)
                    .ThenInclude(a => a!.Item)
                .Include(o => o.Seller)
                .Where(o => o.ExpiresAt <= now)
                .ToArrayAsync(cancellationToken);

            foreach (MarketplaceOffer offer in expiredOffers)
            {
                _marketplaceService.RefundAndRemoveOffer(_db, offer);

                var offeredAsset = offer.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Offered)!;
                var requestedAsset = offer.Assets.FirstOrDefault(a => a.Side == MarketplaceOfferAssetSide.Requested)!;
                _db.ActivityLogs.Add(_activityLogService.CreateMarketplaceOfferExpiredLog(userId: offer.SellerId, offerId: offer.Id, goldFee: offer.GoldFee, offered: offeredAsset, requested: requestedAsset));
                _db.UserNotifications.Add(_userNotificationService.CreateMarketplaceOfferExpiredNotification(userId: offer.SellerId, offerId: offer.Id, goldFee: offer.GoldFee, offered: offeredAsset, requested: requestedAsset));
            }

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("{0} expired marketplace offers were cleaned out", expiredOffers.Length);
            return Result.NoErrors;
        }
    }
}

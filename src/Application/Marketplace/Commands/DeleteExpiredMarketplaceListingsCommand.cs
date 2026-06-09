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

public record DeleteExpiredMarketplaceListingsCommand : IMediatorRequest
{
    internal class Handler(ICrpgDbContext db, IDateTime dateTime, IUserNotificationService userNotificationService, IActivityLogService activityLogService, IMarketplaceService marketplaceService) : IMediatorRequestHandler<DeleteExpiredMarketplaceListingsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DeleteExpiredMarketplaceListingsCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IDateTime _dateTime = dateTime;
        private readonly IUserNotificationService _userNotificationService = userNotificationService;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IMarketplaceService _marketplaceService = marketplaceService;

        public async ValueTask<Result> Handle(DeleteExpiredMarketplaceListingsCommand req, CancellationToken cancellationToken)
        {
            DateTime now = _dateTime.UtcNow;

            MarketplaceListing[] expiredListings = await _db.MarketplaceListings
                .AsSplitQuery()
                .Include(l => l.Assets)
                    .ThenInclude(a => a.UserItem)
                        .ThenInclude(ui => ui!.Item)
                .Include(l => l.Assets)
                    .ThenInclude(a => a!.Item)
                .Include(l => l.Seller)
                .Where(l => l.ExpiresAt <= now)
                .ToArrayAsync(cancellationToken);

            foreach (MarketplaceListing listing in expiredListings)
            {
                _marketplaceService.RefundAndRemoveListing(_db, listing);

                var offeredAsset = listing.Assets.FirstOrDefault(a => a.Side == MarketplaceListingAssetSide.Offered)!;
                var requestedAsset = listing.Assets.FirstOrDefault(a => a.Side == MarketplaceListingAssetSide.Requested)!;
                _db.ActivityLogs.Add(_activityLogService.CreateMarketplaceListingExpiredLog(userId: listing.SellerId, listingId: listing.Id, goldFee: listing.GoldFee, offer: offeredAsset, request: requestedAsset));
                _db.UserNotifications.Add(_userNotificationService.CreateMarketplaceListingExpiredNotification(userId: listing.SellerId, listingId: listing.Id, goldFee: listing.GoldFee, offer: offeredAsset, request: requestedAsset));
            }

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("{0} expired marketplace listings were cleaned out", expiredListings.Length);
            return Result.NoErrors;
        }
    }
}

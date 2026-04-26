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

public record CancelMarketplaceListingCommand : IMediatorRequest
{
    [JsonIgnore]
    public int UserId { get; set; }

    [JsonIgnore]
    public int ListingId { get; set; }

    internal class Handler(ICrpgDbContext db, IActivityLogService activityLogService, IMarketplaceService marketplaceService) : IMediatorRequestHandler<CancelMarketplaceListingCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CancelMarketplaceListingCommand>();
        private readonly ICrpgDbContext _db = db;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IMarketplaceService _marketplaceService = marketplaceService;

        public async ValueTask<Result> Handle(CancelMarketplaceListingCommand req, CancellationToken cancellationToken)
        {
            var listing = await _db.MarketplaceListings
                .AsSplitQuery()
                .Include(l => l.Assets)
                    .ThenInclude(a => a.UserItem)
                        .ThenInclude(ui => ui!.Item)
                .Include(l => l.Assets)
                    .ThenInclude(a => a!.Item)
                .Include(l => l.Seller)
                .FirstOrDefaultAsync(l => l.Id == req.ListingId, cancellationToken);
            if (listing == null)
            {
                return new(CommonErrors.MarketplaceListingNotFound(req.ListingId));
            }

            if (listing.Seller == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (listing.Seller.Id != req.UserId)
            {
                return new(CommonErrors.MarketplaceListingNotAllowed(req.ListingId));
            }

            _marketplaceService.RefundAndRemoveListing(_db, listing);

            _db.ActivityLogs.Add(_activityLogService.CreateMarketplaceListingCancelledLog(userId: req.UserId, listingId: listing.Id, goldFee: listing.GoldFee, offer: listing.Assets.FirstOrDefault(a => a.Side == MarketplaceListingAssetSide.Offered)!, request: listing.Assets.FirstOrDefault(a => a.Side == MarketplaceListingAssetSide.Requested)!));

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Marketplace listing '{0}' cancelled by user '{1}'", listing.Id, req.UserId);
            return Result.NoErrors;
        }
    }
}

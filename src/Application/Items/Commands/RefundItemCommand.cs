using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Items.Commands;

public record RefundItemCommand : IMediatorRequest
{
    [JsonIgnore]
    public string ItemId { get; init; } = string.Empty;
    [JsonIgnore]
    public int UserId { get; init; }

    internal class Handler(ICrpgDbContext db, IActivityLogService activityLogService, IItemService itemService, IMarketplaceService marketplaceService, IUserNotificationService userNotificationService) : IMediatorRequestHandler<RefundItemCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RefundItemCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IItemService _itemService = itemService;
        private readonly IMarketplaceService _marketplaceService = marketplaceService;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IUserNotificationService _userNotificationService = userNotificationService;

        public async ValueTask<Result> Handle(RefundItemCommand req, CancellationToken cancellationToken)
        {
            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
            if (item == null)
            {
                return new(CommonErrors.ItemNotFound(req.ItemId));
            }

            await _itemService.RefundUserItemsByItemAsync(_db, _activityLogService, _userNotificationService, item.Id, cancellationToken);

            await _marketplaceService.InvalidateListingsByItemIdAsync(_db, _activityLogService, _userNotificationService, item.Id, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' refunded item '{1}'", req.UserId, req.ItemId);
            return Result.NoErrors;
        }
    }
}

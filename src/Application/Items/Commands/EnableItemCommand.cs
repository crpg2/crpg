using System.Text.Json.Serialization;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Services;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Items.Commands;

public record EnableItemCommand : IMediatorRequest
{
    [JsonIgnore]
    public string BaseItemId { get; init; } = string.Empty;
    public bool Enable { get; init; }
    [JsonIgnore]
    public int UserId { get; init; }

    internal class Handler(ICrpgDbContext db, IMarketplaceService marketplaceService, IActivityLogService activityLogService, IItemService itemService, IUserNotificationService userNotificationService) : IMediatorRequestHandler<EnableItemCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<EnableItemCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMarketplaceService _marketplaceService = marketplaceService;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly IItemService _itemService = itemService;
        private readonly IUserNotificationService _userNotificationService = userNotificationService;

        public async ValueTask<Result> Handle(EnableItemCommand req, CancellationToken cancellationToken)
        {
            var items = await _db.Items
                .Where(i => i.BaseId == req.BaseItemId)
                .ToListAsync(cancellationToken);
            if (items.Count == 0)
            {
                return new(CommonErrors.ItemNotFound(req.BaseItemId));
            }

            foreach (var item in items)
            {
                await DisableItemAsync(item, req.Enable, cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' {1} item '{2}'", req.UserId,
                req.Enable ? "enabled" : "disabled", req.BaseItemId);
            return Result.NoErrors;
        }

        private async Task DisableItemAsync(Item item, bool enabled, CancellationToken cancellationToken)
        {
            if (enabled)
            {
                item.Enabled = true;
            }
            else
            {
                item.Enabled = false;

                await _db.EquippedItems
                    .RemoveRangeAsync(ei => ei.UserItem!.ItemId == item.Id, cancellationToken);

                await _db.ClanArmoryBorrowedItems
                    .RemoveRangeAsync(bi => bi.UserItem!.ItemId == item.Id, cancellationToken);

                await _db.ClanArmoryItems
                    .RemoveRangeAsync(ci => ci.UserItem!.ItemId == item.Id, cancellationToken);

                await _itemService.RefundUserItemsByItemAsync(_db, _activityLogService, _userNotificationService, item.Id, cancellationToken);

                await _marketplaceService.InvalidateListingsByItemIdAsync(_db, _activityLogService, _userNotificationService, item.Id, cancellationToken);
            }
        }
    }
}

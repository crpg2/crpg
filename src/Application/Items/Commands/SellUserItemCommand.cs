using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Items.Commands;

public record SellUserItemCommand : IMediatorRequest
{
    public int UserItemId { get; init; }
    public int UserId { get; init; }

    internal class Handler(ICrpgDbContext db, IItemService itemService, IActivityLogService activityLogService) : IMediatorRequestHandler<SellUserItemCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<SellUserItemCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IItemService _itemService = itemService;
        private readonly IActivityLogService _activityLogService = activityLogService;

        public async ValueTask<Result> Handle(SellUserItemCommand req, CancellationToken cancellationToken)
        {
            var userItem = await _db.UserItems
                .AsSplitQuery()
                .Include(ui => ui.User)
                .Include(ui => ui.Item)
                .Include(ui => ui.PersonalItem)
                .Include(ui => ui.ClanArmoryItem)
                .Include(ui => ui.ClanArmoryBorrowedItem)
                .Include(ui => ui.MarketplaceListingAssets)
                .FirstOrDefaultAsync(ui => ui.UserId == req.UserId && ui.Id == req.UserItemId, cancellationToken);

            if (userItem == null)
            {
                return new Result(CommonErrors.UserItemNotFound(req.UserItemId));
            }

            if (!userItem.Item!.Enabled)
            {
                return new(CommonErrors.ItemDisabled(userItem.Item.Id));
            }

            if (userItem.Item!.Rank > 0
                || userItem.PersonalItem != null
                || userItem.ClanArmoryItem != null
                || userItem.ClanArmoryBorrowedItem != null
                || userItem.MarketplaceListingAssets.Count != 0)
            {
                return new(CommonErrors.ItemNotSellable(userItem.Item.Id));
            }

            int sellPrice = _itemService.SellUserItem(_db, userItem);

            _db.ActivityLogs.Add(_activityLogService.CreateItemSoldLog(userItem.UserId, userItem.ItemId, sellPrice));

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' sold item '{1}'", req.UserId, userItem.ItemId);
            return new Result();
        }
    }
}

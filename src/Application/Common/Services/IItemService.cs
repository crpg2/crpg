using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities.Items;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Common.Services;

internal interface IItemService
{
    /// <summary>Sells a user item. <see cref="UserItem.Item"/> and <see cref="UserItem.EquippedItems"/> should be loaded.</summary>
    int SellUserItem(ICrpgDbContext db, UserItem userItem);

    void RefundUserItem(ICrpgDbContext db, IActivityLogService activityLogService,
        IUserNotificationService userNotificationService, UserItem userItem);

    /// <summary>Refunds all user items of the given item id. <see cref="UserItem.Item"/> should be loaded for each user item.</summary>
    Task RefundUserItemsByItemAsync(ICrpgDbContext db, IActivityLogService activityLogService,
        IUserNotificationService userNotificationService, string itemId, CancellationToken cancellationToken);
}

internal class ItemService(IDateTime dateTime, Constants constants) : IItemService
{
    private readonly IDateTime _dateTime = dateTime;
    private readonly Constants _constants = constants;

    /// <inheritdoc />
    public int SellUserItem(ICrpgDbContext db, UserItem userItem)
    {
        int price = userItem.Item!.Price;
        // If the item was recently bought it is sold at 100% of its original price.
        int sellPrice = userItem.CreatedAt + TimeSpan.FromMinutes(_constants.ItemSellGracePeriodMinutes) < _dateTime.UtcNow
            ? (int)(price * _constants.ItemSellCostPenalty)
            : price;
        userItem.User!.Gold += sellPrice;
        db.EquippedItems.RemoveRange(userItem.EquippedItems);
        db.UserItems.Remove(userItem);

        return sellPrice;
    }

    public void RefundUserItem(ICrpgDbContext db, IActivityLogService activityLogService,
        IUserNotificationService userNotificationService, UserItem userItem)
    {
        userItem.User!.Gold += userItem.Item!.Price;
        // Trick to avoid UpdatedAt to be updated.
        userItem.User.UpdatedAt = userItem.User.UpdatedAt;
        if (userItem.Item!.Rank > 0)
        {
            userItem.User.HeirloomPoints += userItem.Item!.Rank;
        }

        db.UserItems.Remove(userItem);
        db.ActivityLogs.Add(activityLogService.CreateItemReturnedLog(userItem.User!.Id, userItem.Item!.Id, userItem.Item.Rank, userItem.Item.Price));
        db.UserNotifications.Add(userNotificationService.CreateItemReturnedToUserNotification(userItem.User.Id, userItem.Item.Id, userItem.Item.Rank, userItem.Item.Price));
    }

    public async Task RefundUserItemsByItemAsync(ICrpgDbContext db, IActivityLogService activityLogService,
        IUserNotificationService userNotificationService, string itemId, CancellationToken cancellationToken)
    {
        var userItems = await db.UserItems
            .AsSplitQuery()
            .Include(ui => ui.User)
            .Include(ui => ui.Item)
            .Where(ui => ui.ItemId == itemId)
            .ToArrayAsync(cancellationToken);
        foreach (var userItem in userItems)
        {
            RefundUserItem(db, activityLogService, userNotificationService, userItem);
        }
    }
}

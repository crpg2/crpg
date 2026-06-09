using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Items.Commands;

public record ReforgeUpgradedUserItemCommand : IMediatorRequest<UserItemViewModel>
{
    public int UserItemId { get; init; }
    public int UserId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IActivityLogService activityLogService, Constants constants) : IMediatorRequestHandler<ReforgeUpgradedUserItemCommand, UserItemViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<ReforgeUpgradedUserItemCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IActivityLogService _activityLogService = activityLogService;
        private readonly Constants _constants = constants;

        public async ValueTask<Result<UserItemViewModel>> Handle(ReforgeUpgradedUserItemCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .AsSplitQuery()
                .Include(u => u.Items)
                    .ThenInclude(ui => ui.Item)
                .Include(u => u.Items)
                    .ThenInclude(ui => ui.MarketplaceListingAssets)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);

            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            var userItemToReforge = user.Items
                .FirstOrDefault(ui => ui.Id == req.UserItemId);

            if (userItemToReforge == null)
            {
                return new(CommonErrors.UserItemNotFound(req.UserItemId));
            }

            if (userItemToReforge.IsBroken)
            {
                return new(CommonErrors.ItemBroken(userItemToReforge.ItemId));
            }

            if (userItemToReforge.Item!.Rank == 0)
            {
                return new(CommonErrors.ItemNotReforgeable(userItemToReforge.ItemId));
            }

            if (userItemToReforge.MarketplaceListingAssets.Count != 0)
            {
                return new(CommonErrors.UserItemInMarketplace(userItemToReforge.Id));
            }

            int reforgePrice = (int)_constants.ItemReforgeCostPerRank[userItemToReforge.Item.Rank];

            if (user.Gold < reforgePrice)
            {
                return new(CommonErrors.NotEnoughGold(reforgePrice, user.Gold));
            }

            Item? baseItem = await _db.Items
                .FirstOrDefaultAsync(
                    i => i.BaseId == userItemToReforge.Item!.BaseId && i.Rank == 0,
                    cancellationToken);

            if (baseItem == null)
            {
                return new(CommonErrors.ItemNotReforgeable(userItemToReforge.ItemId));
            }

            _db.ActivityLogs.Add(_activityLogService.CreateItemReforgedLog(user.Id, userItemToReforge.ItemId, userItemToReforge.Item.Rank, reforgePrice));
            user.HeirloomPoints += userItemToReforge.Item.Rank;
            user.Gold -= reforgePrice;
            userItemToReforge.Item = baseItem;

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' has reforged item '{1}'", req.UserId, userItemToReforge.ItemId);
            return new(_mapper.Map<UserItemViewModel>(userItemToReforge));
        }
    }
}

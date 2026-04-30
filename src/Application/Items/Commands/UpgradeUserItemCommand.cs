using System.Text.Json.Serialization;
using AutoMapper;
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

public record UpgradeUserItemCommand : IMediatorRequest<UserItemViewModel>
{
    [JsonIgnore]
    public int UserItemId { get; init; }
    [JsonIgnore]
    public int UserId { get; init; }
    public int UpgradeRank { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IActivityLogService activityLogService) : IMediatorRequestHandler<UpgradeUserItemCommand, UserItemViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpgradeUserItemCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IActivityLogService _activityLogService = activityLogService;

        private readonly int minUpgradeRank = 1;
        private readonly int maxUpgradeRank = 3;

        public async ValueTask<Result<UserItemViewModel>> Handle(UpgradeUserItemCommand req, CancellationToken cancellationToken)
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

            if (req.UpgradeRank < minUpgradeRank || req.UpgradeRank > maxUpgradeRank)
            {
                return new(CommonErrors.InvalidItemUpgradeRank(req.UpgradeRank, minUpgradeRank, maxUpgradeRank));
            }

            var userItemToUpgrade = user.Items
                .FirstOrDefault(ui => ui.Id == req.UserItemId);

            if (userItemToUpgrade == null)
            {
                return new(CommonErrors.UserItemNotFound(req.UserItemId));
            }

            if (userItemToUpgrade.IsBroken)
            {
                return new(CommonErrors.ItemBroken(userItemToUpgrade.ItemId));
            }

            if (userItemToUpgrade.MarketplaceListingAssets.Count != 0)
            {
                return new(CommonErrors.UserItemInMarketplace(userItemToUpgrade.Id));
            }

            if (userItemToUpgrade.Item!.Type == ItemType.Banner)
            {
                return new(CommonErrors.ItemNotUpgradable(userItemToUpgrade.ItemId));
            }

            int upgradeCost = req.UpgradeRank - userItemToUpgrade.Item!.Rank;

            if (user.HeirloomPoints < upgradeCost)
            {
                return new(CommonErrors.NotEnoughHeirloomPoints(upgradeCost, user.HeirloomPoints));
            }

            Item? upgradedItem = await _db.Items
                .FirstOrDefaultAsync(
                    i => i.BaseId == userItemToUpgrade.Item!.BaseId && i.Rank == req.UpgradeRank,
                    cancellationToken);

            if (upgradedItem == null)
            {
                return new(CommonErrors.UserItemMaxRankReached(userItemToUpgrade.Id, userItemToUpgrade.Item!.Rank));
            }

            userItemToUpgrade.Item = upgradedItem;
            user.HeirloomPoints -= upgradeCost;

            _db.ActivityLogs.Add(_activityLogService.CreateItemUpgradedLog(user.Id, userItemToUpgrade.ItemId, req.UpgradeRank));
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' has upgraded item '{1}' by +{2}", req.UserId, req.UserItemId, req.UpgradeRank);
            return new(_mapper.Map<UserItemViewModel>(userItemToUpgrade));
        }
    }
}

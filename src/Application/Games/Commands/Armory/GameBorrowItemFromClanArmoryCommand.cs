using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Games.Commands.Armory;

public record GameBorrowItemFromClanArmoryCommand : IMediatorRequest<ClanArmoryBorrowedItemViewModel>
{
    public int UserItemId { get; init; }
    public int UserId { get; init; }
    public int ClanId { get; init; }

    internal class Handler : IMediatorRequestHandler<GameBorrowItemFromClanArmoryCommand, ClanArmoryBorrowedItemViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<GameBorrowItemFromClanArmoryCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IClanService _clanService;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserNotificationService _userNotificationService;

        public Handler(ICrpgDbContext db, IMapper mapper, IClanService clanService, IActivityLogService activityLogService, IUserNotificationService userNotificationService)
        {
            _db = db;
            _mapper = mapper;
            _clanService = clanService;
            _activityLogService = activityLogService;
            _userNotificationService = userNotificationService;
        }

        public async Task<Result<ClanArmoryBorrowedItemViewModel>> Handle(GameBorrowItemFromClanArmoryCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Where(u => u.Id == req.UserId)
                .Include(u => u.ClanMembership)
                .FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            var clan = await _db.Clans
                .Where(c => c.Id == req.ClanId)
                .FirstOrDefaultAsync(cancellationToken);
            if (clan == null)
            {
                return new(CommonErrors.ClanNotFound(req.ClanId));
            }

            var result = await _clanService.BorrowArmoryItem(_db, clan, user, req.UserItemId, cancellationToken);
            if (result.Errors != null)
            {
                return new(result.Errors);
            }

            _db.ActivityLogs.Add(_activityLogService.CreateBorrowItemFromClanArmoryLog(user.Id, clan.Id, req.UserItemId));
            _db.UserNotifications.Add(_userNotificationService.CreateClanArmoryBorrowItemToLenderNotification(result.Data!.UserItem!.UserId, clan.Id, result.Data!.UserItem.ItemId, user.Id));
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' borrowed item '{1}' from the armory '{2}'", req.UserId, req.UserItemId, req.ClanId);

            return new(_mapper.Map<ClanArmoryBorrowedItemViewModel>(result.Data!));
        }
    }
}

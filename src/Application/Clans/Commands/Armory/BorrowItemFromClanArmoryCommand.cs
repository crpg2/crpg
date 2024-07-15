using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands.Armory;

public record BorrowItemFromClanArmoryCommand : IMediatorRequest<ClanArmoryBorrowedItemViewModel>
{
    public int UserItemId { get; init; }
    public int UserId { get; init; }
    public int ClanId { get; init; }

    internal class Handler : IMediatorRequestHandler<BorrowItemFromClanArmoryCommand, ClanArmoryBorrowedItemViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<BorrowItemFromClanArmoryCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IClanService _clanService;

        public Handler(
            ICrpgDbContext db,
            IMapper mapper,
            IClanService clanService)
        {
            _db = db;
            _mapper = mapper;
            _clanService = clanService;
        }

        public async Task<Result<ClanArmoryBorrowedItemViewModel>> Handle(BorrowItemFromClanArmoryCommand req, CancellationToken cancellationToken)
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

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' borrowed item '{1}' from the armory '{2}'", req.UserId, req.UserItemId, req.ClanId);

            return new(_mapper.Map<ClanArmoryBorrowedItemViewModel>(result.Data!));
        }
    }
}

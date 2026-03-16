using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Clans.Queries;

public record GetClanArmoryQuery : IMediatorRequest<IList<ClanArmoryItemViewModel>>
{
    public int UserId { get; init; }
    public int ClanId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper, IClanService clanService) : IMediatorRequestHandler<GetClanArmoryQuery, IList<ClanArmoryItemViewModel>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;
        private readonly IClanService _clanService = clanService;

        public async ValueTask<Result<IList<ClanArmoryItemViewModel>>> Handle(GetClanArmoryQuery req, CancellationToken cancellationToken)
        {
            var user = await _db.Users.AsNoTracking()
                .Where(u => u.Id == req.UserId)
                .Include(u => u.ClanMembership)
                .FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            var error = _clanService.CheckClanMembership(user, req.ClanId);
            if (error != null)
            {
                return new(error);
            }

            var items = await _db.ClanArmoryItems
                .AsSplitQuery()
                .Where(ci => ci.LenderClanId == req.ClanId)
                .Include(ci => ci.Lender)
                    .ThenInclude(cm => cm!.User)
                .Include(ci => ci.BorrowedItem)
                    .ThenInclude(bi => bi!.Borrower)
                        .ThenInclude(cm => cm!.User)
                .Include(ci => ci.UserItem!)
                    .ThenInclude(ui => ui.Item)
                .ToListAsync(cancellationToken);

            return new(_mapper.Map<IList<ClanArmoryItemViewModel>>(items));
        }
    }
}

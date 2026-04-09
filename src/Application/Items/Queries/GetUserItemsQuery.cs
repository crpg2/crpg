using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Queries;

public record GetUserItemsQuery : IMediatorRequest<IList<UserItemViewModel>>
{
    public int UserId { get; init; }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<GetUserItemsQuery, IList<UserItemViewModel>>
    {
        private readonly ICrpgDbContext _db = db;
        private readonly IMapper _mapper = mapper;

        public async ValueTask<Result<IList<UserItemViewModel>>> Handle(GetUserItemsQuery req, CancellationToken cancellationToken)
        {
            var userItems = await _db.UserItems
                .AsSplitQuery()
                .Include(ui => ui.Item)
                .Include(ui => ui.ClanArmoryItem)
                    .ThenInclude(cai => cai!.Lender)
                        .ThenInclude(cm => cm!.User)
                .Include(ui => ui.PersonalItem)
                .Include(ui => ui.ClanItem)
                .Include(ui => ui.ClanArmoryBorrowedItem)
                .Where(ui =>
                    // Allow disabled items if they are personal- or clan-exclusive.
                    (ui.Item!.Enabled || ui.PersonalItem != null || ui.ClanItem != null)
                    && (ui.UserId == req.UserId || ui.ClanArmoryBorrowedItem!.BorrowerUserId == req.UserId))
                .ToArrayAsync(cancellationToken);

            return new(_mapper.Map<IList<UserItemViewModel>>(userItems));
        }
    }
}

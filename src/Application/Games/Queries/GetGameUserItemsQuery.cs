using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Queries;

public record GetGameUserItemsQuery : IMediatorRequest<IList<UserItemViewModel>>
{
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetGameUserItemsQuery, IList<UserItemViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<UserItemViewModel>>> Handle(GetGameUserItemsQuery req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
               .FirstOrDefaultAsync(u => u.Id == req.UserId,
                   cancellationToken);

            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            var userItems = await _db.UserItems
                .Include(ui => ui.Item)
                .Include(ui => ui.User)
                // .Include(ui => ui.ClanArmoryItem) // TODO: FIXME:
                .Include(ui => ui.PersonalItem)
                .Include(ui => ui.ClanArmoryBorrowedItem)
                .Where(ui =>
                   (ui.Item!.Enabled || ui.PersonalItem != null)
                   && (ui.UserId == user.Id || ui.ClanArmoryBorrowedItem!.BorrowerUserId == user.Id))
                .ToArrayAsync(cancellationToken);

            return new(_mapper.Map<IList<UserItemViewModel>>(userItems));
        }
    }
}

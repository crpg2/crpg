using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Queries;

public record GetGameCharacterEquippedItemsQuery : IMediatorRequest<IList<GameEquippedItemExtendedViewModel>>
{
    public int UserId { get; init; }
    public int CharacterId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetGameCharacterEquippedItemsQuery, IList<GameEquippedItemExtendedViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<GameEquippedItemExtendedViewModel>>> Handle(GetGameCharacterEquippedItemsQuery req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                            .Include(c => c.EquippedItems)
                                .ThenInclude(ei => ei.UserItem)
                                    .ThenInclude(ui => ui!.Item)
                            .Include(c => c.EquippedItems)
                                .ThenInclude(ei => ei.UserItem)
                                    .ThenInclude(ui => ui!.PersonalItem)
                            .Include(c => c.EquippedItems)
                                .ThenInclude(ei => ei.UserItem)
                                    .ThenInclude(ui => ui!.ClanArmoryBorrowedItem)
                            .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            // Filter out null UserItems and disabled items
            var equippedItems = character.EquippedItems
                .Where(ei => ei.UserItem != null &&
                             (ei.UserItem.Item?.Enabled == true || ei.UserItem.PersonalItem != null))
                .ToList();

            return new(_mapper.Map<IList<GameEquippedItemExtendedViewModel>>(equippedItems));
        }
    }
}

using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Commands;

public record UpdateGameCharacterItemsCommand : IMediatorRequest<IList<EquippedItemIdViewModel>>
{
    public int UserId { get; init; }
    public int CharacterId { get; init; }
    public IList<EquippedItemIdViewModel> Items { get; init; } = Array.Empty<EquippedItemIdViewModel>();

    internal class Handler : IMediatorRequestHandler<UpdateGameCharacterItemsCommand, IList<EquippedItemIdViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly ICharacterService _characterService;

        public Handler(ICrpgDbContext db, ICharacterService characterService)
        {
            _db = db;
            _characterService = characterService;
        }

        public async Task<Result<IList<EquippedItemIdViewModel>>> Handle(
            UpdateGameCharacterItemsCommand req,
            CancellationToken cancellationToken)
        {
            // Load character with equipped items and their UserItems
            var character = await _db.Characters
                .Include(c => c.EquippedItems)
                    .ThenInclude(ei => ei.UserItem)
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            // Update equipped items
            var result = await _characterService.UpdateItems(_db, character, req.Items, cancellationToken);

            if (result.Errors is not null)
            {
                return new(result.Errors);
            }

            // Save changes
            await _db.SaveChangesAsync(cancellationToken);

            // Manual projection to DTO
            var equippedItems = character.EquippedItems
                .Where(ei => ei.UserItem != null)
                .Select(ei => new EquippedItemIdViewModel
                {
                    Slot = ei.Slot,
                    UserItemId = ei.UserItem!.Id,
                })
                .ToList();

            return new(equippedItems);
        }
    }
}

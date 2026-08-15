using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Commands;

public record UpdateGameCharacterItemsCommand : IMediatorRequest<IList<GameEquippedItemExtendedViewModel>>
{
    [JsonIgnore]
    public int UserId { get; init; }
    [JsonIgnore]
    public int CharacterId { get; init; }
    public IList<EquippedItemIdViewModel> Items { get; init; } = Array.Empty<EquippedItemIdViewModel>();

    internal class Handler : IMediatorRequestHandler<UpdateGameCharacterItemsCommand, IList<GameEquippedItemExtendedViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly ICharacterService _characterService;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, ICharacterService characterService, IMapper mapper)
        {
            _db = db;
            _characterService = characterService;
            _mapper = mapper;
        }

        public async ValueTask<Result<IList<GameEquippedItemExtendedViewModel>>> Handle(
            UpdateGameCharacterItemsCommand req,
            CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .Include(c => c.EquippedItems)
                    .ThenInclude(ei => ei.UserItem)
                        .ThenInclude(ui => ui!.Item)
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            var result = await _characterService.UpdateItems(_db, character, req.Items, cancellationToken);
            if (result.Errors is not null)
            {
                return new(result.Errors);
            }

            await _db.SaveChangesAsync(cancellationToken);

            return new(_mapper.Map<IList<GameEquippedItemExtendedViewModel>>(character.EquippedItems));
        }
    }
}

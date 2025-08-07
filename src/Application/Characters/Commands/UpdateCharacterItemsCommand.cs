using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands;

public record UpdateCharacterItemsCommand : IMediatorRequest<IList<EquippedItemViewModel>>
{
    [JsonIgnore]
    public int CharacterId { get; init; }

    [JsonIgnore]
    public int UserId { get; init; }

    public IList<EquippedItemIdViewModel> Items { get; init; } = Array.Empty<EquippedItemIdViewModel>();

    internal class Handler : IMediatorRequestHandler<UpdateCharacterItemsCommand, IList<EquippedItemViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterService _characterService;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterService characterService)
        {
            _db = db;
            _mapper = mapper;
            _characterService = characterService;
        }

        public async Task<Result<IList<EquippedItemViewModel>>> Handle(UpdateCharacterItemsCommand req,
            CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .Include(c => c.EquippedItems).ThenInclude(ei => ei.UserItem!.Item)
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            await _characterService.UpdateItems(_db, character, req.Items, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new(_mapper.Map<IList<EquippedItemViewModel>>(character.EquippedItems));
        }
    }
}

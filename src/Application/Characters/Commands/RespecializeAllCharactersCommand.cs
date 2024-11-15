using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands;

public record RespecializeAllCharactersCommand : IMediatorRequest
{
    internal class Handler : IMediatorRequestHandler<RespecializeAllCharactersCommand>
    {
        private readonly ICrpgDbContext _db;
        private readonly ICharacterService _characterService;

        public Handler(ICrpgDbContext db, ICharacterService characterService)
        {
            _db = db;
            _characterService = characterService;
        }

        public async Task<Result> Handle(RespecializeAllCharactersCommand req, CancellationToken cancellationToken)
        {
            var characters = await _db.Characters.ToArrayAsync(cancellationToken: cancellationToken);

            foreach (var character in characters)
            {
                _characterService.ResetCharacterCharacteristics(character, true);
                // Trick to avoid UpdatedAt to be updated.
                character.UpdatedAt = character.UpdatedAt;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }
    }
}

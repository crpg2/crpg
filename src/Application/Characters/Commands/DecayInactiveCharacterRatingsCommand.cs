using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Characters.Commands;

public record DecayInactiveCharacterRatingsCommand : IMediatorRequest
{
    internal class Handler(ICrpgDbContext db, ICharacterService characterService) : IMediatorRequestHandler<DecayInactiveCharacterRatingsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DecayInactiveCharacterRatingsCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly ICharacterService _characterService = characterService;

        public async ValueTask<Result> Handle(DecayInactiveCharacterRatingsCommand req, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var characters = await _db.Characters
                .Include(c => c.Statistics)
                .ToArrayAsync(cancellationToken: cancellationToken);

            int decayedCount = 0;
            foreach (var character in characters)
            {
                decayedCount += _characterService.DecayRatings(character, now);
            }

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Decayed ratings for {Count} character statistics entries", decayedCount);
            return Result.NoErrors;
        }
    }
}

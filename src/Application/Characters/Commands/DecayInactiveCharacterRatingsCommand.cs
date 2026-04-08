using Crpg.Application.Common;
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
    internal class Handler(ICrpgDbContext db, ICharacterService characterService, Constants constants) : IMediatorRequestHandler<DecayInactiveCharacterRatingsCommand>
    {
        private const int BatchSize = 500;
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<DecayInactiveCharacterRatingsCommand>();

        private readonly ICrpgDbContext _db = db;
        private readonly ICharacterService _characterService = characterService;
        private readonly Constants _constants = constants;

        public async ValueTask<Result> Handle(DecayInactiveCharacterRatingsCommand req, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var decayThreshold = now - TimeSpan.FromDays(_constants.RatingDecayDays);

            int decayedCount = 0;
            while (true)
            {
                var characters = await _db.Characters
                    .Include(c => c.Statistics)
                    .Where(c => c.Statistics.Any(s => s.RatingUpdatedAt != null && s.RatingUpdatedAt <= decayThreshold))
                    .OrderBy(c => c.Id)
                    .Take(BatchSize)
                    .ToArrayAsync(cancellationToken);

                if (characters.Length == 0)
                {
                    break;
                }

                foreach (var character in characters)
                {
                    decayedCount += _characterService.DecayRatings(character, now);
                }

                await _db.SaveChangesAsync(cancellationToken);

                // Detach processed entities so the change tracker stays bounded across batches.
                foreach (var character in characters)
                {
                    foreach (var stat in character.Statistics)
                    {
                        _db.Entry(stat).State = EntityState.Detached;
                    }

                    _db.Entry(character).State = EntityState.Detached;
                }

                if (characters.Length < BatchSize)
                {
                    break;
                }
            }

            Logger.LogInformation("Decayed ratings for {Count} character statistics entries", decayedCount);
            return Result.NoErrors;
        }
    }
}

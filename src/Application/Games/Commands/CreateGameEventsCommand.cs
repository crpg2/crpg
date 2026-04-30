using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.GameEvents;
using Crpg.Domain.Entities.Servers;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Games.Commands;

public record CreateGameEventsCommand : IMediatorRequest
{
    public string Instance { get; init; } = string.Empty;
    public GameMode GameMode { get; init; }
    public IList<GameEventViewModel> Events { get; init; } = Array.Empty<GameEventViewModel>();

    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<CreateGameEventsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreateGameEventsCommand>();

        public async ValueTask<Result> Handle(CreateGameEventsCommand req, CancellationToken cancellationToken)
        {
            var gameEvents = req.Events
                .Select(e => new GameEvent
                {
                    GameMode = req.GameMode,
                    Instance = req.Instance,
                    UserId = e.UserId,
                    Type = e.Type,
                    EventData = e.EventData,
                }).ToList();

            db.GameEvents.AddRange(gameEvents);
            await db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("Inserted {0} crpg game events", gameEvents.Count);
            return Result.NoErrors;
        }
    }
}

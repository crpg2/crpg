using Crpg.Application.GameEvents.Commands;
using Mediator;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.WebApi.Workers;

internal class GameEventsCleanerWorker(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private static readonly ILogger Logger = LoggerFactory.CreateLogger<GameEventsCleanerWorker>();

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new DeleteOldGameEventsCommand(), stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured while cleaning old game events");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}

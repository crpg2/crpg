using Crpg.Application.Characters.Commands;
using Mediator;

namespace Crpg.WebApi.Workers;

internal class RatingDecayWorker(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<RatingDecayWorker>();

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new DecayInactiveCharacterRatingsCommand(), stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured while decaying inactive character ratings");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}

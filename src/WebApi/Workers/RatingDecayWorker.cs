using Crpg.Application.Characters.Commands;
using Mediator;

namespace Crpg.WebApi.Workers;

internal class RatingDecayWorker(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<RatingDecayWorker>();

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new DecayInactiveCharacterRatingsCommand(), cancellationToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occurred while decaying inactive character ratings");
            }

            await Task.Delay(TimeSpan.FromHours(24), cancellationToken);
        }
    }
}

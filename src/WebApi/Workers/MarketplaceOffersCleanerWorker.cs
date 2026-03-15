using Crpg.Application.Marketplace.Commands;
using Mediator;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.WebApi.Workers;

public class MarketplaceOffersCleanerWorker(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private static readonly ILogger Logger = LoggerFactory.CreateLogger<MarketplaceOffersCleanerWorker>();

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(new DeleteExpiredMarketplaceOffersCommand(), stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured while cleaning expired marketplace offers");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}

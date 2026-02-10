using Crpg.Application.ActivityLogs.Commands;
using Crpg.Application.IdempotencyKeys.Commands;
using Mediator;

namespace Crpg.WebApi.Workers;

internal class IdempotencyKeysCleanerWorker : BackgroundService
{
    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<IdempotencyKeysCleanerWorker>();

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IdempotencyKeysCleanerWorker(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Send(new DeleteOldIdempotencyKeysCommand(), stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured while cleaning idempotency keys");
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}

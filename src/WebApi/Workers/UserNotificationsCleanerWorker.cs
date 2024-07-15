using Crpg.Application.Notifications.Commands;
using MediatR;

namespace Crpg.WebApi.Workers;

internal class UserNotificationsCleanerWorker : BackgroundService
{
    private static readonly ILogger Logger = Logging.LoggerFactory.CreateLogger<UserNotificationsCleanerWorker>();

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UserNotificationsCleanerWorker(IServiceScopeFactory serviceScopeFactory)
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
                await mediator.Send(new DeleteOldUserNotificationsCommand(), stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured while cleaning user notifications");
            }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }
}

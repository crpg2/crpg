using Crpg.GameServerManager.Api.Models;

namespace Crpg.GameServerManager.Api;

internal interface ICrpgClient : IDisposable
{
    Task<CrpgResult> GetUpcomingStrategusBattles(CrpgRegion region,
        CancellationToken cancellationToken = default);
}

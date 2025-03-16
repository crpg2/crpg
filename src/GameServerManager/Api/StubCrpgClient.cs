using Crpg.GameServerManager.Api.Models;

namespace Crpg.GameServerManager.Api;

internal class StubCrpgClient : ICrpgClient
{
    public Task<CrpgResult> GetUpcomingStrategusBattles(CrpgRegion region,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CrpgResult { });
    }

    public void Dispose()
    {
    }

}

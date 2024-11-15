using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;
internal class CrpgRespawnTimerClient : MissionNetwork
{
    public float RespawnTimer { get; private set; }
    public event Action OnUpdateRespawnTimer = default!;

    public CrpgRespawnTimerClient()
    {
        RespawnTimer = 0;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<CrpgUpdateRespawnTimerMessage>(HandleUpdateRespawnTimer);
    }

    private void HandleUpdateRespawnTimer(CrpgUpdateRespawnTimerMessage message)
    {
        RespawnTimer = message.TimeToSpawn;
        OnUpdateRespawnTimer?.Invoke();
    }
}

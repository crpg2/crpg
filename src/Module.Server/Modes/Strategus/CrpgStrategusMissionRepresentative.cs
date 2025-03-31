using Crpg.Module.Api.Models.Strategus;
using Crpg.Module.Modes.Strategus;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.TrainingGround;

public class CrpgStrategusMissionRepresentative : MissionRepresentativeBase
{
    public BattleSide Side { get; set; }
    public bool IsCommander { get; set; } = false;
#if CRPG_SERVER
    private CrpgStrategusServer _mission = default!;
#endif

    public override void Initialize()
    {
#if CRPG_SERVER
        if (GameNetwork.IsServerOrRecorder)
        {
            _mission = Mission.Current.GetMissionBehavior<CrpgStrategusServer>();
        }
#endif
        Mission.Current.SetMissionMode(MissionMode.Battle, atStart: true);
    }

    public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
    {
        if (GameNetwork.IsClient)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new(mode);

        }
    }
}

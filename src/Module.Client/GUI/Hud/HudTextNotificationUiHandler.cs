using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.Hud;

public class HudTextNotificationUiHandler : MissionView
{
    private GauntletLayer? _gauntletLayer;
    private HudTextNotificationPanelVm? _datasource;

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();
        _datasource = new HudTextNotificationPanelVm();
        try
        {
            _gauntletLayer = new GauntletLayer(100);
            _gauntletLayer.LoadMovie("HudTextNotifications", _datasource);
            MissionScreen.AddLayer(_gauntletLayer);
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"UI crash: {ex.Message} in {GetType().Name}"));
        }
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        HudTextNotificationManager.Instance.Update(dt);
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        HudTextNotificationManager.Cleanup();
        MissionScreen.RemoveLayer(_gauntletLayer);
        _gauntletLayer = null;
    }
}

using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.Hud;

public class CrpgHudTextNotificationUiHandler : MissionView
{
    private GauntletLayer? _gauntletLayer;
    private CrpgHudTextNotificationVm _dataSource;

    public CrpgHudTextNotificationUiHandler()
    {
        _dataSource = new CrpgHudTextNotificationVm(Mission);
        ViewOrderPriority = 2;
    }

    public override void OnMissionScreenInitialize()
    {
        // Initialize Gauntlet UI layer
        try
        {
            _gauntletLayer = new GauntletLayer(ViewOrderPriority);
            var movie = _gauntletLayer.LoadMovie("HudTextNotifications", _dataSource);
            CrpgHudNotificationManager.Instance.Initialize(movie, _gauntletLayer);

            MissionScreen.AddLayer(_gauntletLayer);
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"UI crash HudTextNotificationUiHandler: {ex.Message}"));
            InformationManager.DisplayMessage(new InformationMessage(
                $"UI crash HudTextNotificationUiHandler: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}"));
            Debug.Print($"{ex.Message}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }

        base.OnMissionScreenInitialize();
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        CrpgHudNotificationManager.Instance.Update(dt);
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        if (_gauntletLayer != null)
        {
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }

        CrpgHudNotificationManager.Instance.Cleanup();
        _dataSource!.OnFinalize();
        _dataSource = null!;
    }
}

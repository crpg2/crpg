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
        base.OnMissionScreenInitialize();

        // Initialize Gauntlet UI layer
        try
        {
            _dataSource = new CrpgHudTextNotificationVm(Mission);

            _gauntletLayer = new GauntletLayer(ViewOrderPriority);

            TaleWorlds.GauntletUI.Data.IGauntletMovie movie;
            try
            {
                movie = _gauntletLayer.LoadMovie("HudTextNotifications", _dataSource);
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"Failed to load HudTextNotifications movie: {ex.Message}"));
                throw;
            }

            CrpgHudNotificationManager.Instance.Initialize(movie, _gauntletLayer);

            try
            {
                MissionScreen.AddLayer(_gauntletLayer);
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"Failed to add Gauntlet layer: {ex.Message}"));
                throw;
            }
        }
        catch (Exception ex)
        {
            InformationManager.DisplayMessage(new InformationMessage($"UI crash HudTextNotificationUiHandler: {ex.Message}"));
            InformationManager.DisplayMessage(new InformationMessage(
                $"UI crash HudTextNotificationUiHandler: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}"));
            Debug.Print($"{ex.Message}\n{ex.StackTrace}", 0, Debug.DebugColor.DarkBlue);
        }
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

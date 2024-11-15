using Crpg.Module.Common;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI;

internal class CrpgRespawnTimerUiHandler : MissionView
{
    private GauntletLayer? _gauntletLayer;
    private CrpgRespawnTimerClient? _respawnTimerClient;
    private CrpgRespawnTimerVm? _dataSource;
    private SpriteCategory? _mpMissionCategory;

    public CrpgRespawnTimerUiHandler()
    {
        ViewOrderPriority = 5;
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        _mpMissionCategory = UIResourceManager.SpriteData.SpriteCategories["ui_mpmission"];
        _mpMissionCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);

        _dataSource = new CrpgRespawnTimerVm(Mission);
        _gauntletLayer = new GauntletLayer(ViewOrderPriority + 1);
        _gauntletLayer.LoadMovie("CrpgRespawnTimer", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
        _respawnTimerClient = Mission.GetMissionBehavior<CrpgRespawnTimerClient>();
        _respawnTimerClient.OnUpdateRespawnTimer += OnUpdateRespawnTimer;
    }

    public override void OnMissionScreenTick(float dt)
    {
        _dataSource!.Tick(dt);
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        MissionScreen.RemoveLayer(_gauntletLayer);
        if (_respawnTimerClient != null)
        {
            _respawnTimerClient.OnUpdateRespawnTimer -= OnUpdateRespawnTimer;
        }

        _mpMissionCategory?.Unload();
    }

    private void OnUpdateRespawnTimer()
    {
        _dataSource!.Update();
    }
}

using Crpg.Module.Modes.Dtv;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI.Dtv;

internal class DtvHudUiHandler : MissionView
{
    private DtvHudVm? _dataSource;
    private GauntletLayer? _gauntletLayer;
    private SpriteCategory? _mpMissionCategory;
    private CrpgDtvClient? _dtvClient;

    public DtvHudUiHandler()
    {
        ViewOrderPriority = 2;
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        _mpMissionCategory = UIResourceManager.SpriteData.SpriteCategories["ui_mpmission"];
        _mpMissionCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);

        _dataSource = new DtvHudVm(Mission);
        _gauntletLayer = new GauntletLayer(ViewOrderPriority);
        _gauntletLayer.LoadMovie("DtvHud", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
    }

    public override void AfterStart()
    {
        base.AfterStart();
        _dtvClient = Mission.GetMissionBehavior<CrpgDtvClient>();
        if (_dtvClient != null)
        {
            _dtvClient.OnUpdateCurrentProgress += OnUpdateProgress;
            _dtvClient.OnRoundStart += OnUpdateProgress;
            _dtvClient.OnWaveStart += OnUpdateProgress;
        }
    }

    public override void OnMissionScreenFinalize()
    {
        MissionScreen.RemoveLayer(_gauntletLayer);
        _dataSource!.OnFinalize();
        _mpMissionCategory?.Unload();
        base.OnMissionScreenFinalize();
        if (_dtvClient != null)
        {
            _dtvClient.OnUpdateCurrentProgress -= OnUpdateProgress;
            _dtvClient.OnRoundStart -= OnUpdateProgress;
            _dtvClient.OnWaveStart -= OnUpdateProgress;
        }
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        _dataSource!.Tick(dt);
    }

    private void OnUpdateProgress()
    {
        _dataSource!.UpdateProgress();
    }
}

using Crpg.Module.Modes.Strategus;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI.Strategus;

internal class StrategusHudUiHandler : MissionView
{
    private StrategusHudVm? _dataSource;
    private GauntletLayer? _gauntletLayer;
    private SpriteCategory? _mpMissionCategory;
    private CrpgStrategusClient? _stratClient;

    public StrategusHudUiHandler()
    {
        ViewOrderPriority = 2;
    }

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        _mpMissionCategory = UIResourceManager.SpriteData.SpriteCategories["ui_mpmission"];
        _mpMissionCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);

        _dataSource = new StrategusHudVm(Mission);
        _gauntletLayer = new GauntletLayer(ViewOrderPriority);
        _gauntletLayer.LoadMovie("StrategusHud", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
    }

    public override void AfterStart()
    {
        base.AfterStart();
        _stratClient = Mission.GetMissionBehavior<CrpgStrategusClient>();
        if (_stratClient != null)
        {
           _stratClient.OnUpdateTicketCount += OnUpdateTicketCount;
        }
    }

    public override void OnMissionScreenFinalize()
    {
        MissionScreen.RemoveLayer(_gauntletLayer);
        _dataSource!.OnFinalize();
        _mpMissionCategory?.Unload();
        base.OnMissionScreenFinalize();
        if (_stratClient != null)
        {
            _stratClient.OnUpdateTicketCount -= OnUpdateTicketCount;
        }
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        _dataSource!.Tick(dt);
    }

    private void OnUpdateTicketCount()
    {
        _dataSource!.UpdateTicketCount();
    }
}

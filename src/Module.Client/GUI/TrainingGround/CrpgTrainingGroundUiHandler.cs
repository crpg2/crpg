using Crpg.Module.Modes.TrainingGround;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.TwoDimension;

namespace Crpg.Module.GUI.TrainingGround;

public class CrpgTrainingGroundUiHandler : MissionView
{
    private CrpgTrainingGroundVm? _dataSource;
    private GauntletLayer? _gauntletLayer;
    private SpriteCategory? _mpMissionCategory;
    private CrpgTrainingGroundMissionMultiplayerClient? _client;
    private MissionLobbyEquipmentNetworkComponent? _equipmentController;
    private MissionLobbyComponent? _lobbyComponent;
    private bool _isPeerEquipmentsDirty;

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();
        ViewOrderPriority = 15;
        _client = Mission.GetMissionBehavior<CrpgTrainingGroundMissionMultiplayerClient>();
        _dataSource = new(MissionScreen.CombatCamera, _client);
        _gauntletLayer = new GauntletLayer("TrainingGroundHud", ViewOrderPriority);
        _gauntletLayer.LoadMovie("TrainingGroundHud", _dataSource);
        SpriteData spriteData = UIResourceManager.SpriteData;
        TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
        ResourceDepot uIResourceDepot = UIResourceManager.ResourceDepot;
        _mpMissionCategory = spriteData.SpriteCategories["ui_mpmission"];
        _mpMissionCategory.Load(resourceContext, uIResourceDepot);
        MissionScreen.AddLayer(_gauntletLayer);
        _equipmentController = Mission.GetMissionBehavior<MissionLobbyEquipmentNetworkComponent>();
        _equipmentController.OnEquipmentRefreshed += OnEquipmentRefreshed;
        MissionPeer.OnEquipmentIndexRefreshed += OnPeerEquipmentIndexRefreshed;
        _lobbyComponent = Mission.GetMissionBehavior<MissionLobbyComponent>();
        _lobbyComponent.OnPostMatchEnded += OnPostMatchEnded;
        NativeOptions.OnNativeOptionChanged = (NativeOptions.OnNativeOptionChangedDelegate)Delegate.Combine(NativeOptions.OnNativeOptionChanged, new NativeOptions.OnNativeOptionChangedDelegate(OnNativeOptionChanged));
        _dataSource.IsEnabled = true;
        _isPeerEquipmentsDirty = true;
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        MissionScreen.RemoveLayer(_gauntletLayer);
        _mpMissionCategory?.Unload();
        _dataSource!.OnFinalize();
        _dataSource = null;
        _gauntletLayer = null;
        _equipmentController!.OnEquipmentRefreshed -= OnEquipmentRefreshed;
        MissionPeer.OnEquipmentIndexRefreshed -= OnPeerEquipmentIndexRefreshed;
        _lobbyComponent!.OnPostMatchEnded -= OnPostMatchEnded;
        NativeOptions.OnNativeOptionChanged = (NativeOptions.OnNativeOptionChangedDelegate?)Delegate.Remove(NativeOptions.OnNativeOptionChanged, new NativeOptions.OnNativeOptionChangedDelegate(OnNativeOptionChanged));
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);
        _dataSource!.Tick(dt);
        if (_client?.MyRepresentative?.ControlledAgent != null && Input.IsGameKeyReleased(13))
        {
            _client.MyRepresentative.OnInteraction();
        }

        if (_isPeerEquipmentsDirty)
        {
            _dataSource.Markers.RefreshPeerEquipments();
            _isPeerEquipmentsDirty = false;
        }
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
        if (affectedAgent == Agent.Main)
        {
            _dataSource!.OnMainAgentRemoved();
        }
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);
        if (agent == Agent.Main)
        {
            _dataSource!.OnMainAgentBuild();
        }
    }

    public override void OnFocusGained(Agent agent, IFocusable focusableObject, bool isInteractable)
    {
        base.OnFocusGained(agent, focusableObject, isInteractable);
        if (!(focusableObject is Agent))
        {
            _dataSource!.Markers.OnFocusGained();
        }
    }

    public override void OnFocusLost(Agent agent, IFocusable focusableObject)
    {
        base.OnFocusLost(agent, focusableObject);
        if (focusableObject is not Agent)
        {
            _dataSource!.Markers.OnFocusLost();
        }
    }

    public void OnPeerEquipmentIndexRefreshed(MissionPeer peer, int equipmentSetIndex)
    {
        _dataSource!.Markers.OnPeerEquipmentRefreshed(peer);
    }

    private void OnNativeOptionChanged(NativeOptions.NativeOptionsType optionType)
    {
        if (optionType == NativeOptions.NativeOptionsType.ScreenResolution)
        {
            _dataSource!.OnScreenResolutionChanged();
        }
    }

    private void OnEquipmentRefreshed(MissionPeer peer)
    {
        _dataSource!.Markers.OnPeerEquipmentRefreshed(peer);
    }

    private void OnPostMatchEnded()
    {
        _dataSource!.IsEnabled = false;
    }
}

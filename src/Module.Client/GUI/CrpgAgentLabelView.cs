using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace Crpg.Module.GUI;

/// <summary>
/// Displays a team banner circle above agents' heads. Based on the native MissionAgentLabelView.
/// Alive: shows labels for allies only.
/// Dead or on spectator team: shows labels for both teams.
/// Photo mode: labels are hidden.
/// </summary>
internal class CrpgAgentLabelView : MissionView
{
    private const float HighlightedLabelScaleFactor = 20f;
    private const float LabelBannerWidth = 0.4f;
    private const float LabelBlackBorderWidth = 0.44f;
    private const float NearDistance = 1.5f;
    private const float FarDistance = 8f;

    private static readonly Vec3 MeshOffset = new(0f, 0f, 2f);

    private readonly Dictionary<Agent, MetaMesh> _agentMeshes = [];
    private readonly Dictionary<Texture, Material> _labelMaterials = [];
    private readonly List<Agent> _nearbyAgentsWithMeshes = [];

    private bool _isSuspendingView;
    private bool _isResumingView;
    private bool _alwaysShowBanners;
    private bool _indicatorsActive;

    private bool IndicatorsActive
    {
        get => _indicatorsActive;
        set
        {
            if (_indicatorsActive != value)
            {
                _indicatorsActive = value;
                UpdateAllAgentMeshVisibilities();
            }
        }
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        Mission.Teams.OnPlayerTeamChanged += OnPlayerTeamChanged;
        Mission.OnMainAgentChanged += OnMainAgentChanged;
        ManagedOptions.OnManagedOptionChanged += OnManagedOptionChanged;
        MissionScreen.OnSpectateAgentFocusIn += OnSpectateAgentFocusIn;
        MissionScreen.OnSpectateAgentFocusOut += OnSpectateAgentFocusOut;
    }

    public override void AfterStart()
    {
        UpdateAlwaysShowBannersSetting();
    }

    public override void OnMissionTick(float dt)
    {
        UpdateNearbyBannerOpacities();
        IndicatorsActive = _alwaysShowBanners || Input.IsGameKeyDown(5);
    }

    public override void OnRemoveBehavior()
    {
        UnregisterEvents();
        base.OnRemoveBehavior();
    }

    public override void OnMissionScreenFinalize()
    {
        UnregisterEvents();
        base.OnMissionScreenFinalize();
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
    {
        RemoveAgentLabel(affectedAgent);
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        InitAgentLabel(agent);
    }

    public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
    {
        RemoveAgentLabel(agent);
        InitAgentLabel(agent);
    }

    public override void OnClearScene()
    {
        _agentMeshes.Clear();
        _labelMaterials.Clear();
        _nearbyAgentsWithMeshes.Clear();
    }

    public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
    {
        base.OnMissionModeChange(oldMissionMode, atStart);
        UpdateAllAgentMeshVisibilities();
    }

    public override void OnPhotoModeActivated()
    {
        base.OnPhotoModeActivated();
        UpdateAllAgentMeshVisibilities();
    }

    public override void OnPhotoModeDeactivated()
    {
        base.OnPhotoModeDeactivated();
        UpdateAllAgentMeshVisibilities();
    }

    protected override void OnSuspendView()
    {
        base.OnSuspendView();
        _isSuspendingView = true;
        UpdateAllAgentMeshVisibilities();
        _isSuspendingView = false;
    }

    protected override void OnResumeView()
    {
        base.OnResumeView();
        _isResumingView = true;
        UpdateAllAgentMeshVisibilities();
        _isResumingView = false;
    }

    private void UnregisterEvents()
    {
        if (Mission != null)
        {
            Mission.Teams.OnPlayerTeamChanged -= OnPlayerTeamChanged;
            Mission.OnMainAgentChanged -= OnMainAgentChanged;
        }

        ManagedOptions.OnManagedOptionChanged -= OnManagedOptionChanged;

        if (MissionScreen != null)
        {
            MissionScreen.OnSpectateAgentFocusIn -= OnSpectateAgentFocusIn;
            MissionScreen.OnSpectateAgentFocusOut -= OnSpectateAgentFocusOut;
        }
    }

    private void InitAgentLabel(Agent agent)
    {
        if (!agent.IsHuman || agent.AgentVisuals == null)
        {
            return;
        }

        Banner? banner = ResolveBanner(agent);
        if (banner == null)
        {
            return;
        }

        MetaMesh? labelMesh = MetaMesh.GetCopy("troop_banner_selection", showErrors: false, mayReturnNull: true);
        Material? baseMaterial = Material.GetFromResource("agent_label_with_tableau");
        if (labelMesh == null || baseMaterial == null)
        {
            return;
        }

        BannerDebugInfo debugInfo = BannerDebugInfo.CreateManual(GetType().Name);
        Texture? bannerTexture = banner.GetTableauTextureSmall(in debugInfo, null);
        Texture fallbackTexture = Texture.GetFromResource("banner_top_of_head");

        Texture cacheKey = bannerTexture ?? fallbackTexture;
        if (_labelMaterials.TryGetValue(cacheKey, out Material? cachedMaterial))
        {
            baseMaterial = cachedMaterial;
        }
        else
        {
            baseMaterial = baseMaterial.CreateCopy();
            Material materialToUpdate = baseMaterial;
            debugInfo = BannerDebugInfo.CreateManual(GetType().Name);
            bannerTexture = banner.GetTableauTextureSmall(in debugInfo, tex =>
            {
                materialToUpdate.SetTexture(Material.MBTextureType.DiffuseMap, tex);
            });
            baseMaterial.SetTexture(Material.MBTextureType.DiffuseMap2, fallbackTexture);
            _labelMaterials.Add(bannerTexture ?? fallbackTexture, baseMaterial);
        }

        labelMesh.SetMaterial(baseMaterial);
        labelMesh.SetVectorArgument(0.5f, 0.5f, 0.25f, 0.25f);
        agent.AgentVisuals.AddMultiMesh(labelMesh, BodyMeshTypes.Label);
        _agentMeshes[agent] = labelMesh;
        UpdateMeshVisibility(agent);
        SetBannerOpacity(agent, highlighted: false);
    }

    private static Banner? ResolveBanner(Agent agent)
    {
        return agent.Team?.Banner;
    }

    private void RemoveAgentLabel(Agent agent)
    {
        if (agent.IsHuman && _agentMeshes.TryGetValue(agent, out MetaMesh? mesh))
        {
            agent.AgentVisuals?.ReplaceMeshWithMesh(mesh, null, BodyMeshTypes.Label);
            _agentMeshes.Remove(agent);
        }

        _nearbyAgentsWithMeshes.Remove(agent);
    }

    private void UpdateMeshVisibility(Agent agent)
    {
        if (agent.IsActive() && _agentMeshes.TryGetValue(agent, out MetaMesh? mesh))
        {
            bool visible = ShouldShowLabel(agent);
            mesh.SetVisibilityMask(visible ? VisibilityMaskFlags.Final : 0);
        }
    }

    private bool ShouldShowLabel(Agent agent)
    {
        if (!_isResumingView && (IsViewSuspended || _isSuspendingView))
        {
            return false;
        }

        if (MissionScreen.IsPhotoModeEnabled)
        {
            return false;
        }

        if (BannerlordConfig.FriendlyTroopsBannerOpacity <= 0f)
        {
            return false;
        }

        if (MissionScreen.LastFollowedAgent == agent)
        {
            return false;
        }

        if (!_indicatorsActive)
        {
            return false;
        }

        // Dead or on the spectator team: show labels for both teams.
        if (IsSpectating())
        {
            return agent.Team != null;
        }

        // Alive: show only for allies.
        return IsAlly(agent);
    }

    private bool IsAlly(Agent agent)
    {
        if (agent.Team == null || Mission == null || agent == Mission.MainAgent)
        {
            return false;
        }

        if (!GameNetwork.IsSessionActive)
        {
            return agent.Team == Mission.PlayerTeam || agent.Team == Mission.PlayerAllyTeam;
        }

        Team? myTeam = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.Team;
        return myTeam != null && agent.Team == myTeam;
    }

    private bool IsSpectating()
    {
        if (!GameNetwork.IsSessionActive || !GameNetwork.IsMyPeerReady)
        {
            return Mission?.MainAgent == null;
        }

        MissionPeer? missionPeer = GameNetwork.MyPeer?.GetComponent<MissionPeer>();
        Team? myTeam = missionPeer?.Team;

        // On the spectator team or dead (no controlled agent).
        return myTeam == null || myTeam == Mission.SpectatorTeam || missionPeer?.ControlledAgent == null;
    }

    private void UpdateNearbyBannerOpacities()
    {
        foreach (Agent agent in _nearbyAgentsWithMeshes)
        {
            SetBannerOpacity(agent, highlighted: false);
        }

        _nearbyAgentsWithMeshes.Clear();

        AgentProximityMap.ProximityMapSearchStruct search =
            AgentProximityMap.BeginSearch(Mission, MissionScreen.CombatCamera.Position.AsVec2, FarDistance, extendRangeByBiggestAgentCollisionPadding: false);

        while (search.LastFoundAgent != null)
        {
            if (_agentMeshes.ContainsKey(search.LastFoundAgent))
            {
                _nearbyAgentsWithMeshes.Add(search.LastFoundAgent);
            }

            AgentProximityMap.FindNext(Mission, ref search);
        }

        foreach (Agent agent in _nearbyAgentsWithMeshes)
        {
            SetBannerOpacity(agent, highlighted: false);
        }
    }

    private void SetBannerOpacity(Agent agent, bool highlighted)
    {
        if (!_agentMeshes.TryGetValue(agent, out MetaMesh? mesh))
        {
            return;
        }

        float opacitySign = highlighted ? 1f : -1f;
        float distance = (agent.Position + MeshOffset).Distance(MissionScreen.CombatCamera.Position);

        if (distance < NearDistance)
        {
            opacitySign = 0f;
        }
        else if (distance < FarDistance)
        {
            opacitySign *= (distance - NearDistance) / (FarDistance - NearDistance);
        }

        mesh.SetVectorArgument2(HighlightedLabelScaleFactor, LabelBannerWidth, LabelBlackBorderWidth,
            opacitySign * BannerlordConfig.FriendlyTroopsBannerOpacity);
    }

    private void UpdateAllAgentMeshVisibilities()
    {
        foreach (Agent agent in Mission.Agents)
        {
            if (agent.IsHuman)
            {
                UpdateMeshVisibility(agent);
            }
        }
    }

    private void OnPlayerTeamChanged(Team previousTeam, Team currentTeam)
    {
        UpdateAllAgentMeshVisibilities();
    }

    private void OnMainAgentChanged(Agent oldAgent)
    {
        UpdateAllAgentMeshVisibilities();
    }

    private void OnSpectateAgentFocusIn(Agent agent)
    {
        UpdateMeshVisibility(agent);
    }

    private void OnSpectateAgentFocusOut(Agent agent)
    {
        UpdateMeshVisibility(agent);
    }

    private void OnManagedOptionChanged(ManagedOptions.ManagedOptionsType optionType)
    {
        if (optionType == ManagedOptions.ManagedOptionsType.AlwaysShowFriendlyTroopBannersType
            || optionType == ManagedOptions.ManagedOptionsType.FriendlyTroopsBannerOpacity)
        {
            UpdateAlwaysShowBannersSetting();
            UpdateAllAgentMeshVisibilities();
        }
    }

    private void UpdateAlwaysShowBannersSetting()
    {
        float config = ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.AlwaysShowFriendlyTroopBannersType);
        _alwaysShowBanners = config == 2f || (config == 1f && GameNetwork.IsMultiplayer);
    }
}

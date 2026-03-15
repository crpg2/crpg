using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace Crpg.Module.GUI;

/// <summary>
/// Displays a banner circle above ally agents' heads. Based on the native MissionAgentLabelView.
/// On a playing team (alive or dead): shows labels for allies using clan banners.
/// On the spectator team: shows labels for both teams using each team's banner.
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
    private bool _wasOnSpectatorTeam;

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
        bool isOnSpectatorTeam = IsOnSpectatorTeam();
        if (isOnSpectatorTeam != _wasOnSpectatorTeam)
        {
            _wasOnSpectatorTeam = isOnSpectatorTeam;
            ReinitializeAllLabels();
        }

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
        InitAgentLabel(agent, banner);
    }

    public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
    {
        RemoveAgentLabel(agent);
        InitAgentLabel(agent, null);
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

    private void InitAgentLabel(Agent agent, Banner? peerBanner)
    {
        if (!agent.IsHuman)
        {
            return;
        }

        Banner? banner = ResolveBanner(agent, peerBanner);
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

    /// <summary>
    /// On the spectator team: use the team banner so both teams are distinguishable.
    /// On a playing team (alive or dead): use the peer/clan banner (original behavior).
    /// </summary>
    private Banner? ResolveBanner(Agent agent, Banner? peerBanner)
    {
        if (IsOnSpectatorTeam())
        {
            return agent.Team?.Banner;
        }

        return peerBanner ?? agent.Origin?.Banner;
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

        // On the spectator team, show labels for both teams.
        if (IsOnSpectatorTeam())
        {
            return agent.Team != null;
        }

        // On a playing team (alive or dead), show only for allies.
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

    private bool IsOnSpectatorTeam()
    {
        if (!GameNetwork.IsSessionActive || !GameNetwork.IsMyPeerReady)
        {
            return false;
        }

        Team? myTeam = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.Team;
        return myTeam == null || myTeam == Mission.SpectatorTeam;
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

    /// <summary>
    /// Removes and re-creates all labels. Used when switching between the spectator team and a playing team
    /// since the banner source changes (team banner vs clan banner).
    /// </summary>
    private void ReinitializeAllLabels()
    {
        List<Agent> agents = _agentMeshes.Keys.ToList();
        foreach (Agent agent in agents)
        {
            RemoveAgentLabel(agent);
        }

        _labelMaterials.Clear();

        foreach (Agent agent in Mission.Agents)
        {
            if (agent.IsHuman)
            {
                InitAgentLabel(agent, null);
            }
        }
    }

    private void OnPlayerTeamChanged(Team previousTeam, Team currentTeam)
    {
        ReinitializeAllLabels();
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

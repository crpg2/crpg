using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class FireWeaponsBehaviorServer : MissionNetwork
{
    // torch_fire_smoke
    // psys_game_burning_agent
    internal static readonly string[] ParticleSystemIds =
    [
        "psys_game_burning_agent",
        "torch_fire_smoke",
        "torch_fire_sparks",
        // "water_splash_2", // just a splash
        // "prt_torch_flame", // real smokey
        "psys_campfire",
        // "firefly_a",
        // "psys_campfire_big", // crashes
        // "psys_blacksmith_smoke", // crashes
        "psys_game_water_splash_1",
        "small_torch_flame",
        "aserai_torch_fire_spark",
        // add more as needed
    ];
    public struct AgentFireWeaponData
    {
        public static readonly AgentFireWeaponData Default = new()
        {
            Enabled = false,
            ParticleSystemId = "psys_game_burning_agent",
            LightColor = new Vec3(1f, 0.5f, 20f),
            LightRadius = 20.0f,
            LightIntensity = 100f,
        };

        public bool Enabled { get; set; }
        public string ParticleSystemId { get; set; }
        public Vec3 LightColor { get; set; }
        public float LightRadius { get; set; }
        public float LightIntensity { get; set; }
    }

    private readonly Dictionary<NetworkCommunicator, AgentFireWeaponData> _peerFireWeaponData = [];
    private readonly Dictionary<Agent, AgentFireWeaponData> _botFireWeaponData = [];
    private bool _fireWeaponAllEnabled = false;

    public void ToggleFireWeaponAll(bool? state = null)
    {
        bool targetState = state ?? !_fireWeaponAllEnabled;

        _fireWeaponAllEnabled = targetState;

        foreach (var agent in Mission.Current.Agents)
        {
            if (agent == null)
            {
                continue;
            }

            ToggleFireWeapon(agent, targetState);
        }
    }

    public AgentFireWeaponData GetData(Agent agent)
    {
        var peer = GetPeer(agent);
        if (peer != null)
        {
            return _peerFireWeaponData.TryGetValue(peer, out var peerData) ? peerData : AgentFireWeaponData.Default;
        }

        return _botFireWeaponData.TryGetValue(agent, out var botData) ? botData : AgentFireWeaponData.Default;
    }

    public AgentFireWeaponData GetData(NetworkCommunicator peer)
    {
        return _peerFireWeaponData.TryGetValue(peer, out var data) ? data : AgentFireWeaponData.Default;
    }

    public void SetData(Agent agent, AgentFireWeaponData data)
    {
        var peer = GetPeer(agent);
        if (peer != null)
        {
            _peerFireWeaponData[peer] = data;
        }
        else
        {
            _botFireWeaponData[agent] = data;
        }

        BroadcastAgentUpdateFireWeaponMessage(agent);
    }

    public void SetData(NetworkCommunicator peer, AgentFireWeaponData data)
    {
        _peerFireWeaponData[peer] = data;

        var agent = peer.ControlledAgent;
        if (agent != null)
        {
            BroadcastAgentUpdateFireWeaponMessage(agent);
        }
    }

    public bool IsFireWeaponEnabled(Agent agent)
    {
        var peer = GetPeer(agent);
        if (peer != null)
        {
            return _peerFireWeaponData.TryGetValue(peer, out var peerData) && peerData.Enabled;
        }

        return _botFireWeaponData.TryGetValue(agent, out var botData) && botData.Enabled;
    }

    public bool IsFireWeaponEnabled(NetworkCommunicator peer)
    {
        return _peerFireWeaponData.TryGetValue(peer, out var data) && data.Enabled;
    }

    public bool IsFireWeaponAllEnabled()
    {
        return _fireWeaponAllEnabled;
    }

    public void ToggleFireWeapon(Agent agent, bool? state = null)
    {
        var data = GetData(agent);
        bool targetState = state ?? !data.Enabled;
        data.Enabled = targetState;
        SetData(agent, data);
    }

    public void SendAgentUpdateFireWeaponMessageToPeer(NetworkCommunicator peer, Agent agent)
    {
        if (peer == null || !peer.IsConnectionActive)
        {
            return;
        }

        var data = GetData(agent);

        GameNetwork.BeginModuleEventAsServer(peer);
        GameNetwork.WriteMessage(new UpdateFireWeapon
        {
            AgentIndex = agent.Index,
            Enabled = data.Enabled,
            ParticleSystemId = data.ParticleSystemId,
            LightColor = new Vec3(
                Math.Clamp(data.LightColor.x, 0f, 1f),
                Math.Clamp(data.LightColor.y, 0f, 1f),
                Math.Clamp(data.LightColor.z, 0f, 1f)),
            LightRadius = Math.Clamp(data.LightRadius, 0f, 100f),
            LightIntensity = Math.Clamp(data.LightIntensity, 0f, 100f),
        });
        GameNetwork.EndModuleEventAsServer();
    }

    public void BroadcastAgentUpdateFireWeaponMessage(Agent agent)
    {
        var data = GetData(agent);

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateFireWeapon
        {
            AgentIndex = agent.Index,
            Enabled = data.Enabled,
            ParticleSystemId = data.ParticleSystemId,
            LightColor = new Vec3(
                Math.Clamp(data.LightColor.x, 0f, 255f),
                Math.Clamp(data.LightColor.y, 0f, 255f),
                Math.Clamp(data.LightColor.z, 0f, 255f)),
            LightRadius = Math.Clamp(data.LightRadius, 0f, 100f),
            LightIntensity = Math.Clamp(data.LightIntensity, 0f, 100f),
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);

        if (!GameNetwork.IsServer)
        {
            return;
        }

        // Apply global all state first
        if (_fireWeaponAllEnabled)
        {
            ToggleFireWeapon(agent, true);
            return; // already handled, no need to check individual data
        }

        // Reapply individual state on respawn
        var peer = GetPeer(agent);
        if (peer != null)
        {
            if (_peerFireWeaponData.TryGetValue(peer, out var peerData) && peerData.Enabled)
            {
                BroadcastAgentUpdateFireWeaponMessage(agent);
            }
        }
        else
        {
            if (_botFireWeaponData.TryGetValue(agent, out var botData) && botData.Enabled)
            {
                BroadcastAgentUpdateFireWeaponMessage(agent);
            }
        }
    }

    public override void OnAgentDeleted(Agent affectedAgent)
    {
        base.OnAgentDeleted(affectedAgent);
        _botFireWeaponData.Remove(affectedAgent); // only removes bots, peer data persists
    }

    public override void OnPlayerDisconnectedFromServer(NetworkCommunicator peer)
    {
        base.OnPlayerDisconnectedFromServer(peer);
        _peerFireWeaponData.Remove(peer);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<UserRequestUpdateFireWeapons>(HandleUserRequestUpdateFireWeapons);
    }

    private static NetworkCommunicator? GetPeer(Agent agent) => agent.MissionPeer?.GetNetworkPeer();
    private bool HandleUserRequestUpdateFireWeapons(NetworkCommunicator networkPeer, UserRequestUpdateFireWeapons message)
    {
        if (networkPeer == null || !networkPeer.IsConnectionActive || !networkPeer.IsSynchronized)
        {
            return true;
        }

        var entries = new List<UpdateAllFireWeapons.FireWeaponData>();

        // Add all active agents from peer data
        foreach (var kvp in _peerFireWeaponData.Where(kvp => kvp.Value.Enabled))
        {
            if (kvp.Key == null || !kvp.Key.IsConnectionActive)
            {
                continue;
            }

            var agent = kvp.Key.ControlledAgent;
            if (agent == null)
            {
                continue;
            }

            entries.Add(new UpdateAllFireWeapons.FireWeaponData
            {
                AgentIndex = agent.Index,
                Enabled = kvp.Value.Enabled,
                ParticleSystemId = kvp.Value.ParticleSystemId,
                LightColor = kvp.Value.LightColor,
                LightRadius = kvp.Value.LightRadius,
                LightIntensity = kvp.Value.LightIntensity,
            });
        }

        // Add all active bots
        foreach (var kvp in _botFireWeaponData.Where(kvp => kvp.Value.Enabled))
        {
            if (kvp.Key == null || !kvp.Key.IsActive())
            {
                continue;
            }

            entries.Add(new UpdateAllFireWeapons.FireWeaponData
            {
                AgentIndex = kvp.Key.Index,
                Enabled = kvp.Value.Enabled,
                ParticleSystemId = kvp.Value.ParticleSystemId,
                LightColor = kvp.Value.LightColor,
                LightRadius = kvp.Value.LightRadius,
                LightIntensity = kvp.Value.LightIntensity,
            });
        }

        if (entries.Count == 0)
        {
            return true;
        }

        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new UpdateAllFireWeapons { Entries = entries });
        GameNetwork.EndModuleEventAsServer();

        return true;
    }
}

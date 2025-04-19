using System.ComponentModel;
using System.Net.Mail;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common;
internal class AmmoQuiverChangeMissionBehavior : MissionBehavior
{
    public delegate void MissileShotHandler(Agent shooterAgent, EquipmentIndex weaponIndex);
    public delegate void WieldedItemChangedHandler(EquipmentIndex newWeaponIndex, MissionWeapon weapon);
    public delegate void ItemDropHandler(Agent agent, SpawnedItemEntity spawnedItem);
    public delegate void ItemPickupHandler(Agent agent, SpawnedItemEntity spawnedItem);
    public event MissileShotHandler OnMissileShot = (index, data) => { }; // Lambda initialization
    public event WieldedItemChangedHandler WieldedItemChanged = (index, data) => { };
    public event ItemDropHandler OnItemDrop = (agent, spawnedItem) => { };
    public event ItemPickupHandler OnItemPickUp = (agent, spawnedItem) => { };
    public delegate void AmmoQuiverChangedHandler(Agent agent);
    public event AmmoQuiverChangedHandler OnAmmoQuiverChanged = (agent) => { };

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    private const bool IsDebugEnabled = false;
    private static int _instanceCount = 0;
    private readonly GameNetwork.NetworkMessageHandlerRegisterer _networkMessageHandlerRegisterer;
    private MissionTime _lastMissileShotTime = MissionTime.Zero;

    public AmmoQuiverChangeMissionBehavior()
    {
        _networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        _instanceCount++;
        LogDebug($"AmmoQuiverChangeMissionBehavior instance {_instanceCount} created (Hash: {GetHashCode()})");
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        if (agent != null && agent.IsActive() && agent == Mission.MainAgent)
        {
            EquipmentIndex wieldedWeaponIndex = EquipmentIndex.None;
            MissionWeapon mWeaponWielded = MissionWeapon.Invalid;

            wieldedWeaponIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            if (wieldedWeaponIndex == EquipmentIndex.None)
            {
                return;
            }

            mWeaponWielded = agent.Equipment[wieldedWeaponIndex];
            if (mWeaponWielded.IsEmpty || mWeaponWielded.Item == null)
            {
                return;
            }

            // Notify listeners (e.g MissionView)
            LogDebug($"MB: OnAgentBuild()");
            // OnWeaponChanged(wieldedWeaponIndex, mWeaponWielded);
            OnMainAgentWieldedItemChange();
        }
    }

    public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
    {
        // notify listeners
        if (shooterAgent == Agent.Main)
        {
            if (MissionTime.Now - _lastMissileShotTime < MissionTime.Seconds(2f / 60f))
            {
                return;
            }

            _lastMissileShotTime = MissionTime.Now;

            LogDebug($"MB: OnAgentShootMissile() - for main agent only");
            OnMissileShot(shooterAgent, weaponIndex);
        }
    }

    public override void OnBehaviorInitialize()
    {
        LogDebug($"MB: OnBehaviorInitialize()");
        if (!GameNetwork.IsServer)
        {
            Debug.Print("Registering CustomServerMessage handler on client.", 0, Debug.DebugColor.Green);
            _networkMessageHandlerRegisterer.Register<CustomServerMessage>(HandleCustomServerMessage);
        }

        if (Mission.Current != null)
        {
            Mission.Current.OnItemDrop += HandleItemDrop;
            Mission.Current.OnItemPickUp += HandleItemPickup;
        }

        OnAmmoQuiverChanged += HandleAmmoQuiverChange;

        base.OnBehaviorInitialize();
        InitializeMainAgentPropterties();
    }

    public override void OnRemoveBehavior()
    {
        LogDebug($"MB: OnRemoveBehavior()");

        if (Agent.Main != null)
        {
            Agent.Main.OnMainAgentWieldedItemChange -= OnMainAgentWieldedItemChange;
        }

        if (Mission.Current != null)
        {
            Mission.Current.OnMainAgentChanged -= OnMainAgentChanged;
            Mission.Current.OnItemDrop -= HandleItemDrop;
            Mission.Current.OnItemPickUp -= HandleItemPickup;
        }

        OnAmmoQuiverChanged -= HandleAmmoQuiverChange;

        OnMissileShot = (index, data) => { };
        WieldedItemChanged = (index, data) => { };
        OnItemDrop = (agent, spawnedItem) => { };
        OnItemPickUp = (agent, spawnedItem) => { };
        OnAmmoQuiverChanged = (agent) => { };

        _instanceCount--;
        base.OnRemoveBehavior();
    }

    public void InitializeMainAgentPropterties()
    {
        LogDebug($"MB: InitializeMainAgentPropterties()");
        Mission.Current.OnMainAgentChanged -= OnMainAgentChanged;
        Mission.Current.OnMainAgentChanged += OnMainAgentChanged;

        OnMainAgentChanged(null, null);
    }

    public bool RequestChangeRangedAmmo()
    {
        LogDebug("RequestChangeRangedAmmo()");
        Agent agent = Agent.Main;

        // Check if agent is wielding a weapon that uses quiver. bow xbow or musket
        if (agent == null || !agent.IsActive() || !AmmoQuiverChangeComponent.IsAgentWieldedWeaponRangedUsesQuiver(agent, out EquipmentIndex wieldedWeaponIndex, out MissionWeapon mWeaponWielded, out bool isThrowingWeapon))
        {
            LogDebug("RequestChangeRangedAmmo(): IsAgentWieldedWeaponRangedUsesQuiver() failed");
            return false;
        }

        // check agent quivers
        if (!AmmoQuiverChangeComponent.GetAgentQuiversWithAmmoEquippedForWieldedWeapon(agent, out List<int> ammoQuivers))
        {
            LogDebug("RequestChangeRangedAmmo(): GetAgentQuiversWithAmmoEquippedForWieldedWeapon() failed");
            return false;
        }

        // multiple quivers with ammo found
        if (ammoQuivers.Count() > 1)
        {
            LogDebug("RequestChangeRangedAmmo() Quivers Found: " + ammoQuivers.Count());
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new ClientRequestAmmoQuiverChange());
            GameNetwork.EndModuleEventAsClient();
            return true;
        }
        else
        {
            LogDebug("RequestChangeRangedAmmo(): ammoQuivers.Count() < 1");
        }

        return false;
    }

    private void HandleItemDrop(Agent agent, SpawnedItemEntity spawnedItem)
    {
        if (agent != null && spawnedItem != null && Mission.MainAgent != null && agent == Mission.MainAgent && agent.IsActive())
        {
            LogDebug($"MB: HandleItemDrop() - for main agent only");
            OnItemDrop?.Invoke(agent, spawnedItem);
        }
    }

    private void HandleItemPickup(Agent agent, SpawnedItemEntity spawnedItem)
    {
        if (agent != null && spawnedItem != null && Mission.MainAgent != null && agent == Mission.MainAgent && Mission.MainAgent.IsActive())
        {
            LogDebug($"MB: HandleItemPickup() - for main agent only");
            OnItemPickUp?.Invoke(agent, spawnedItem);
        }
    }

    private void HandleAmmoQuiverChange(Agent agent)
    {
        LogDebug($"MB: AmmoQuiverChangedHandler");
    }

    private void HandleCustomServerMessage(CustomServerMessage message)
    {
        LogDebug($"HandleCustomServerMessage: {message.Message}");
        if (message.Message == "AmmoQuiverChanged")
        {
            OnAmmoQuiverChanged?.Invoke(Agent.Main);
        }
    }

    private void OnMainAgentChanged(object? sender, PropertyChangedEventArgs? e)
    {
        LogDebug("MB: OnMainAgentChanged()");
        if (Agent.Main != null)
        {
            // Prevent duplicate subscriptions
            Agent.Main.OnMainAgentWieldedItemChange -= OnMainAgentWieldedItemChange;
            Agent.Main.OnMainAgentWieldedItemChange += OnMainAgentWieldedItemChange;

            OnMainAgentWieldedItemChange(); // called once when agent changes
        }
    }

    private void OnMainAgentWieldedItemChange()
    {
        LogDebug($"MB: OnMainAgentWieldedItemChange() ");

        Agent agent = Agent.Main;
        if (agent == null || !agent.IsActive() || agent.Equipment == null)
        {
            return;
        }

        EquipmentIndex wieldedWeaponIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        MissionWeapon weapon = wieldedWeaponIndex == EquipmentIndex.None
            ? MissionWeapon.Invalid
            : agent.Equipment[wieldedWeaponIndex];

        WieldedItemChanged?.Invoke(wieldedWeaponIndex, weapon);
    }

#pragma warning disable CS0162 // Unreachable code if debug disabled
    private void LogDebug(string message)
    {
        if (IsDebugEnabled)
        {
            InformationManager.DisplayMessage(new InformationMessage($"[DEBUG] {message}"));
        }
    }
#pragma warning restore CS0162

}

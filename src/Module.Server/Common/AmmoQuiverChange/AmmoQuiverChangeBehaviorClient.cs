using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static Crpg.Module.Common.AmmoQuiverChange.AmmoQuiverChangeComponent;

namespace Crpg.Module.Common.AmmoQuiverChange;
internal class AmmoQuiverChangeBehaviorClient : MissionNetwork
{
    public event Action<QuiverEventType, object[]>? OnQuiverEvent;

    public enum QuiverEventType
    {
        AmmoQuiverChanged,
        WieldedItemChanged,
        ItemDrop,
        ItemPickup,
        AgentBuild,
        AgentRemoved,
        AgentChanged,
        MissileShot,
        AgentStatusChanged,
        AmmoCountIncreased,
        AmmoCountDecreased,
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    private const float QuiverChangeWindowSeconds = 1.5f;
    private const int QuiverChangeMaxCount = 3;
    private readonly string _changedSuccessSound = "event:/mission/combat/pickup_arrows";
    private readonly string _changeDeniedSound = "event:/ui/panels/previous";
    private MissionTime _lastMissileShotTime = MissionTime.Zero;
    private MissionTime _lastAmmoChangeTime = MissionTime.Zero;
    private int _quiverChangeCount;

    private bool _wasMainAgentActive;
    private int _lastKnownTotalAmmo = -1;

    public override void OnMissionTick(float dt)
    {
        Agent mainAgent = Agent.Main;
        bool isActive = mainAgent?.IsActive() == true;

        if (_wasMainAgentActive != isActive)
        {
            _wasMainAgentActive = isActive;
            TriggerQuiverEvent(QuiverEventType.AgentStatusChanged, isActive);
        }

        if (mainAgent != null && isActive)
        {
            int currentAmmo = GetTotalAmmoCount();

            if (currentAmmo > _lastKnownTotalAmmo)
            {
                TriggerQuiverEvent(QuiverEventType.AmmoCountIncreased);
            }
            else if (currentAmmo < _lastKnownTotalAmmo)
            {
                TriggerQuiverEvent(QuiverEventType.AmmoCountDecreased);
            }

            _lastKnownTotalAmmo = currentAmmo;
        }
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        if (agent != null && agent.IsActive() && agent == Mission.MainAgent)
        {
            EquipmentIndex wieldedWeaponIndex = agent.GetPrimaryWieldedItemIndex();
            if (wieldedWeaponIndex == EquipmentIndex.None)
            {
                return;
            }

            MissionWeapon mWeaponWielded = agent.Equipment[wieldedWeaponIndex];
            if (mWeaponWielded.IsEmpty || mWeaponWielded.Item == null)
            {
                return;
            }

            TriggerQuiverEvent(QuiverEventType.AgentBuild, banner);
            OnMainAgentWieldedItemChangeHandler();
        }
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
        if (affectedAgent == Agent.Main)
        {
            TriggerQuiverEvent(QuiverEventType.AgentRemoved, affectedAgent, affectorAgent, agentState, blow);
        }
    }

    public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
    {
        if (shooterAgent == Agent.Main)
        {
            // rate limit a bit was getting double triggered
            if (MissionTime.Now - _lastMissileShotTime < MissionTime.Seconds(2f / 60f))
            {
                return;
            }

            _lastMissileShotTime = MissionTime.Now;
            TriggerQuiverEvent(QuiverEventType.MissileShot, shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
        }
    }

    public override void OnBehaviorInitialize()
    {
        if (Mission.Current != null)
        {
            Mission.Current.OnItemDrop += OnItemDropHandler;
            Mission.Current.OnItemPickUp += OnItemPickupHandler;
            Mission.Current.OnMainAgentChanged += OnMainAgentChangedHandler;

            OnMainAgentChangedHandler(null);
        }

        base.OnBehaviorInitialize();
    }

    public override void OnRemoveBehavior()
    {
        if (Agent.Main != null)
        {
            Agent.Main.OnMainAgentWieldedItemChange -= OnMainAgentWieldedItemChangeHandler;
        }

        if (Mission.Current != null)
        {
            Mission.Current.OnMainAgentChanged -= OnMainAgentChangedHandler;
            Mission.Current.OnItemDrop -= OnItemDropHandler;
            Mission.Current.OnItemPickUp -= OnItemPickupHandler;
        }

        base.OnRemoveBehavior();
    }

    public bool RequestChangeRangedAmmo()
    {
        Agent agent = Agent.Main;

        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        if (!IsAgentWieldedWeaponRangedUsesQuiver(agent, out EquipmentIndex wieldedWeaponIndex, out MissionWeapon wieldedWeapon, out bool isThrowingWeapon))
        {
            return false;
        }

        if (!CheckAmmoChangeSpam())
        {
            PlaySoundForMainAgent(_changeDeniedSound);
            return false;
        }

        // check agent quivers
        if (!GetAgentQuiversWithAmmoEquippedForWieldedWeapon(agent, out List<int> ammoQuivers))
        {
            PlaySoundForMainAgent(_changeDeniedSound);
            return false;
        }

        // not enough quivers with ammo found
        if (ammoQuivers.Count < 2)
        {
            PlaySoundForMainAgent(_changeDeniedSound);
            return false;
        }

        // attack release phase ineligible - throwing release
        if (agent.GetCurrentActionType(1) == Agent.ActionCodeType.ReleaseThrowing)
        {
            PlaySoundForMainAgent(_changeDeniedSound);
            return false;
        }

        if (!wieldedWeapon.IsEmpty &&
            !wieldedWeapon.IsEqualTo(MissionWeapon.Invalid) &&
            wieldedWeapon.Item is { } weaponItem)
        {
            switch (weaponItem.Type)
            {
                case ItemObject.ItemTypeEnum.Crossbow when wieldedWeapon.ReloadPhase == 2: // Loaded
                    PlaySoundForMainAgent(_changeDeniedSound);
                    return false;

                case ItemObject.ItemTypeEnum.Musket when wieldedWeapon.ReloadPhase == 1: // Loaded
                    PlaySoundForMainAgent(_changeDeniedSound);
                    return false;
            }
        }

        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new AmmoQuiverChangeRequestClientMessage());
        GameNetwork.EndModuleEventAsClient();

        return true;
    }

    public int GetTotalAmmoCount()
    {
        Agent agent = Agent.Main;
        if (agent == null || !agent.IsActive())
        {
            return -1;
        }

        int totalAmmo = 0;

        // Iterate through the agent's equipment
        for (int i = 0; i < 4; i++)
        {
            var eItem = agent.Equipment[i];
            if (eItem.IsEmpty || eItem.Item == null)
            {
                continue;
            }

            totalAmmo += eItem.Amount;
        }

        return totalAmmo;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsClient)
        {
            base.AddRemoveMessageHandlers(registerer);
            registerer.Register<AmmoQuiverChangeSuccessServerMessage>(HandleQuiverChangeSuccessMessage);
        }
    }

    private bool CheckAmmoChangeSpam()
    {
        var now = MissionTime.Now;
        if (now - _lastAmmoChangeTime > MissionTime.Seconds(QuiverChangeWindowSeconds))
        {
            _lastAmmoChangeTime = now;
            _quiverChangeCount = 1;
            return true;
        }

        if (_quiverChangeCount < QuiverChangeMaxCount)
        {
            _quiverChangeCount++;
            return true;
        }

        return false;
    }

    private void PlaySoundForMainAgent(string soundEventString)
    {
        Agent agent = Agent.Main;
        if (agent == null || !agent.IsActive())
        {
            return;
        }

        Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(soundEventString), agent.Position, false, true, -1, -1);
    }

    private void TriggerQuiverEvent(QuiverEventType type, params object[] parameters)
    {
        OnQuiverEvent?.Invoke(type, parameters);
    }

    private void OnItemDropHandler(Agent agent, SpawnedItemEntity spawnedItem)
    {
        if (agent != null && spawnedItem != null && Mission.MainAgent != null && agent == Mission.MainAgent && agent.IsActive())
        {
            TriggerQuiverEvent(QuiverEventType.ItemDrop, agent, spawnedItem);
        }
    }

    private void OnItemPickupHandler(Agent agent, SpawnedItemEntity spawnedItem)
    {
        if (agent != null && spawnedItem != null && Mission.MainAgent != null && agent == Mission.MainAgent && Mission.MainAgent.IsActive())
        {
            TriggerQuiverEvent(QuiverEventType.ItemPickup, agent, spawnedItem);
        }
    }

    private void OnMainAgentChangedHandler(Agent? oldAgent)
    {
        if (Agent.Main != null)
        {
            // Prevent duplicate subscriptions
            Agent.Main.OnMainAgentWieldedItemChange -= OnMainAgentWieldedItemChangeHandler;
            Agent.Main.OnMainAgentWieldedItemChange += OnMainAgentWieldedItemChangeHandler;

            TriggerQuiverEvent(QuiverEventType.AgentChanged);

            OnMainAgentWieldedItemChangeHandler(); // called once when agent changes
            _lastKnownTotalAmmo = GetTotalAmmoCount();
        }
    }

    private void OnMainAgentWieldedItemChangeHandler()
    {
        Agent agent = Agent.Main;
        if (agent == null || !agent.IsActive() || agent.Equipment == null)
        {
            return;
        }

        EquipmentIndex wieldedWeaponIndex = agent.GetPrimaryWieldedItemIndex();
        MissionWeapon weapon = wieldedWeaponIndex == EquipmentIndex.None
            ? MissionWeapon.Invalid
            : agent.Equipment[wieldedWeaponIndex];

        TriggerQuiverEvent(QuiverEventType.WieldedItemChanged, wieldedWeaponIndex, weapon);
    }

    private void HandleQuiverChangeSuccessMessage(AmmoQuiverChangeSuccessServerMessage message)
    {
        TriggerQuiverEvent(QuiverEventType.AmmoQuiverChanged);

        if (IsAgentWieldedWeaponRangedUsesQuiver(Agent.Main, out _, out _, out bool isThrowingWeapon) && !isThrowingWeapon)
        {
            PlaySoundForMainAgent(_changedSuccessSound);
        }
    }
}

using System.Runtime.InteropServices;
using Mono.Cecil.Cil;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;
internal class AmmoQuiverChangeComponent : MissionNetwork
{
    private readonly Dictionary<PlayerId, ChangeStatus> _wantsToChangeAmmoQuiver;
    public AmmoQuiverChangeComponent()
    {
        _wantsToChangeAmmoQuiver = new Dictionary<PlayerId, ChangeStatus>();
    }

    public static bool IsQuiverItem(ItemObject item)
    {
        return item != null && (
               item.Type == ItemObject.ItemTypeEnum.Arrows ||
               item.Type == ItemObject.ItemTypeEnum.Bolts ||
               item.Type == ItemObject.ItemTypeEnum.Bullets);
    }

    public static EquipmentIndex GetEquippedQuiverItemIndex(Agent agent)
    {
        if (agent != null && agent.IsActive())
        {
            MissionEquipment equipment = agent.Equipment;

            // List to store quiver indexes
            var ammoQuivers = new System.Collections.Generic.List<int>();

            // Loop through equipment and find quivers
            for (int i = 0; i < 4; i++)
            {
                var item = equipment[i].Item;
                // Check if item is a quiver and not empty
                if (item != null && !equipment[i].IsEmpty && IsQuiverItem(item))
                {
                    return (EquipmentIndex)i;
                }
            }
        }

        return EquipmentIndex.None;
    }

    public static bool GetAgentQuiversEquipped(Agent agent, out List<int> ammoQuivers)
    {
        // List to store quiver indexes
        ammoQuivers = new System.Collections.Generic.List<int>();

        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        MissionEquipment equipment = agent.Equipment;

        // Loop through equipment and find quivers
        for (int i = 0; i < 4; i++)
        {
            var item = equipment[i].Item;
            // Check if item is a quiver and not empty
            if (item != null && !equipment[i].IsEmpty && IsQuiverItem(item))
            {
                ammoQuivers.Add(i);
            }
        }

        return true;
    }

    public static bool GetAgentQuiversWithAmmoEquippedForWieldedWeapon(Agent agent, out List<int> ammoQuivers)
    {
        // List to store quiver indexes
        ammoQuivers = new System.Collections.Generic.List<int>();

        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        if (!IsAgentWieldedWeaponRangedUsesQuiver(agent, out EquipmentIndex wieldedWeaponIndex, out MissionWeapon wieldedWeapon))
        {
            return false;
        }

        MissionEquipment equipment = agent.Equipment;
        // Loop through equipment and find quivers
        for (int i = 0; i < 4; i++)
        {
            MissionWeapon iWeapon = equipment[i];
            ItemObject item = iWeapon.Item;
            // Check if item is a quiver and not empty
            if (item != null && !iWeapon.IsEmpty && IsQuiverItem(item))
            {
                // check that its a quiver for weapon
                if (!IsQuiverAmmoWeaponForRangedWeapon(iWeapon, wieldedWeapon))
                {
                    continue;
                }

                // check that it has ammo left in quiver
                if (iWeapon.Amount > 0)
                {
                    ammoQuivers.Add(i);
                }
            }
        }

        return true;
    }

    public static bool IsAgentWieldedWeaponRangedUsesQuiver(Agent agent, out EquipmentIndex wieldedWeaponIndex, out MissionWeapon wieldedWeapon) // Bow Xbow or Musket
    {
        wieldedWeaponIndex = EquipmentIndex.None;
        wieldedWeapon = MissionWeapon.Invalid;

        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        wieldedWeaponIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        if (wieldedWeaponIndex == EquipmentIndex.None)
        {
            return false;
        }

        if (!agent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand).IsRangedWeapon) // is ranged weapon
        {
            return false;
        }

        wieldedWeapon = agent.Equipment[wieldedWeaponIndex];
        if (wieldedWeapon.IsEmpty || wieldedWeapon.Item == null)
        {
            return false;
        }

        var type = wieldedWeapon.Item.Type;
        return type == ItemObject.ItemTypeEnum.Bow ||
                   type == ItemObject.ItemTypeEnum.Crossbow ||
                   type == ItemObject.ItemTypeEnum.Musket;
    }

    public static bool IsQuiverAmmoWeaponForRangedWeapon(MissionWeapon mAmmo, MissionWeapon mWeapon)
    {
        if (mAmmo.IsEmpty || mWeapon.IsEmpty)
        {
            return false;
        }

        mWeapon.GatherInformationFromWeapon(out bool weaponHasMelee, out bool weaponHasShield, out bool weaponHasPolearm, out bool weaponHasNonConsumableRanged, out bool weaponHasThrown, out WeaponClass rangedAmmoClass);
        WeaponComponentData mAmmoData = mAmmo.GetWeaponComponentDataForUsage(0);
        WeaponClass mAmmoClass = mAmmoData.WeaponClass;

        if (mAmmoClass == rangedAmmoClass)
        {
            return true;
        }

        return false;
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        // if user has an arrow loaded or reloading
        foreach (var networkPeer in GameNetwork.NetworkPeers)
        {
            var playerId = networkPeer.VirtualPlayer.Id;
            var agent = networkPeer.ControlledAgent;

            if (agent == null || !agent.IsActive()) // agent no longer exists/spawned?
            {
                _wantsToChangeAmmoQuiver.Remove(playerId);
                continue;
            }

            if (!_wantsToChangeAmmoQuiver.TryGetValue(playerId, out var wantsToChangeAmmoQuiver)) // not requesting
            {
                continue;
            }

            if (wantsToChangeAmmoQuiver.AmmoChangeRequested) // requested to change quivers
            {
                // ammo not loaded in weapon or reloading or changed weapons
                if (!IsAgentRangedWeaponLoadedOrLoading(agent, out bool notRangedWeapon))
                {
                    if (notRangedWeapon) // changed weapon waiting for a quiver change (ie loaded xbow)
                    {
                        _wantsToChangeAmmoQuiver.Remove(playerId); // cancel quiver change request
                    }
                    else
                    {
                        ExecuteClientAmmoQuiverChange(networkPeer);
                    }
                }
            }
        }
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsServer)
        {
            registerer.Register<ClientRequestAmmoQuiverChange>(HandleClientEventRequestAmmoQuiverChange);
        }
    }

    private static bool IsAgentRangedWeaponLoadedOrLoading(Agent agent, out bool notRangedWeapon)
    {
        notRangedWeapon = false;
        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        if (IsAgentWieldedWeaponRangedUsesQuiver(agent, out EquipmentIndex wieldedWeaponIndex, out MissionWeapon wieldedWeapon))
        {
            var itemType = wieldedWeapon.Item.Type;
            if (itemType == ItemObject.ItemTypeEnum.Bow)
            {
                if (wieldedWeapon.ReloadPhase != 0)
                {
                    return true;
                }
            }
            else if (itemType == ItemObject.ItemTypeEnum.Crossbow || itemType == ItemObject.ItemTypeEnum.Musket)
            {
                if (agent.GetCurrentActionType(1) == Agent.ActionCodeType.Reload)
                {
                    return true;
                }

                if (wieldedWeapon.ReloadPhase != 0)
                {
                    return true;
                }
            }
        }
        else
        { // maybe changed weapons with a xbow bolt loaded should cancel request to reduce checks on tick?
            notRangedWeapon = true;
            return false;
        }

        return false;
    }

    private bool HandleClientEventRequestAmmoQuiverChange(NetworkCommunicator peer, GameNetworkMessage baseMessage)
    {
        Agent agent = peer.ControlledAgent;

        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        PlayerId playerId = peer.VirtualPlayer.Id;

        // check if loaded ammo in weapon -- set flag and wait until shot or weapon changed to execute
        if (!_wantsToChangeAmmoQuiver.TryGetValue(playerId, out var lastActiveStatus))
        {
            _wantsToChangeAmmoQuiver[playerId] = new ChangeStatus
            {
                AmmoChangeRequested = true,
            };
        }

        return true;
    }

    private void ExecuteClientAmmoQuiverChange(NetworkCommunicator peer)
    {
        Agent agent = peer.ControlledAgent;

        if (agent == null || !agent.IsActive())
        {
            return;
        }

        MissionEquipment equipment = agent.Equipment;

        // List to store quiver indexes
        var ammoQuivers = new System.Collections.Generic.List<int>();

        // Loop through equipment and find quivers
        for (int i = 0; i < 4; i++)
        {
            var item = equipment[i].Item;
            // Check if item is a quiver and not empty
            if (item != null && !equipment[i].IsEmpty && IsQuiverItem(item))
            {
                ammoQuivers.Add(i);
            }
        }

        // If there are more than 1 but fewer than 4 quivers, perform swaps
        if (ammoQuivers.Count > 1 && ammoQuivers.Count < 4)
        {
            SwapQuivers(agent, equipment, ammoQuivers);
        }

        // Handle the request to change ammo quiver
        var playerId = peer.VirtualPlayer.Id;
        if (_wantsToChangeAmmoQuiver.Remove(playerId))
        {
            // If the request exists, we remove it
        }

        agent.UpdateWeapons();
        /*
        EquipmentIndex ammoIndex = GetEquippedQuiverItemIndex(agent);
        int ammoCount = agent.Equipment[ammoIndex].Ammo;
        int ammoCount2 = agent.Equipment[ammoIndex].Amount;
        if (ammoIndex != EquipmentIndex.None)
        {
            TaleWorlds.Library.Debug.Print("ExecuteClientAmmoQuiverChange()  ammoIndex: " + ammoIndex + " ammoCount: " + ammoCount + " ammoCount2: " + ammoCount2);
            agent.AgentVisuals.UpdateQuiverMeshesWithoutAgent((int)ammoIndex, ammoCount2); // might need to do all quivers
        }
        */
    }

    private void SwapQuivers(Agent agent, MissionEquipment equipment, System.Collections.Generic.List<int> ammoQuivers)
    {
        if (agent == null || !agent.IsActive())
        {
            return;
        }

        int count = ammoQuivers.Count;

        if (count == 2)
        {
            // Swap between two quivers
            MissionWeapon firstWeapon = equipment[ammoQuivers[0]];
            MissionWeapon secondWeapon = equipment[ammoQuivers[1]];

            agent.EquipWeaponWithNewEntity((EquipmentIndex)ammoQuivers[0], ref secondWeapon);
            agent.EquipWeaponWithNewEntity((EquipmentIndex)ammoQuivers[1], ref firstWeapon);
        }
        else if (count == 3)
        {
            // Rotate three quivers
            MissionWeapon firstWeapon = equipment[ammoQuivers[0]];
            MissionWeapon secondWeapon = equipment[ammoQuivers[1]];
            MissionWeapon thirdWeapon = equipment[ammoQuivers[2]];

            agent.EquipWeaponWithNewEntity((EquipmentIndex)ammoQuivers[0], ref secondWeapon);
            agent.EquipWeaponWithNewEntity((EquipmentIndex)ammoQuivers[1], ref thirdWeapon);
            agent.EquipWeaponWithNewEntity((EquipmentIndex)ammoQuivers[2], ref firstWeapon);
        }
    }

    private struct ChangeStatus
    {
        public bool AmmoChangeRequested { get; set; }
    }
}

// Handle Network Message
[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class ClientRequestAmmoQuiverChange : GameNetworkMessage
{
    protected override void OnWrite()
    {
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Mission;
    }

    protected override string OnGetLogFormat()
    {
        return "Request to change ammo quiver";
    }
}

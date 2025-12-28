using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.AmmoQuiverChange;
internal class AmmoQuiverChangeComponent : MissionNetwork
{
    public AmmoQuiverChangeComponent()
    {
    }

    public static bool IsQuiverItem(ItemObject item)
    {
        return item != null && (
               item.Type == ItemObject.ItemTypeEnum.Arrows ||
               item.Type == ItemObject.ItemTypeEnum.Bolts ||
               item.Type == ItemObject.ItemTypeEnum.Bullets ||
               item.Type == ItemObject.ItemTypeEnum.Thrown);
    }

    public static bool GetAgentQuiversWithAmmoEquippedForWieldedWeapon(Agent agent, out List<int> ammoQuivers)
    {
        // List to store quiver indexes
        ammoQuivers = new System.Collections.Generic.List<int>();

        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        if (!IsAgentWieldedWeaponRangedUsesQuiver(agent, out EquipmentIndex wieldedWeaponIndex, out MissionWeapon wieldedWeapon, out bool isThrowingWeapon))
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

    public static bool IsAgentWieldedWeaponRangedUsesQuiver(Agent agent, out EquipmentIndex wieldedWeaponIndex, out MissionWeapon wieldedWeapon, out bool isThrowingWeapon) // Bow Xbow or Musket
    {
        wieldedWeaponIndex = EquipmentIndex.None;
        wieldedWeapon = MissionWeapon.Invalid;
        isThrowingWeapon = false;

        if (agent == null || !agent.IsActive())
        {
            return false;
        }

        wieldedWeaponIndex = agent.GetPrimaryWieldedItemIndex();
        if (wieldedWeaponIndex == EquipmentIndex.None)
        {
            return false;
        }

        wieldedWeapon = agent.Equipment[wieldedWeaponIndex];
        if (wieldedWeapon.IsEmpty || wieldedWeapon.Item == null)
        {
            return false;
        }

        var type = wieldedWeapon.Item.Type;
        if (type == ItemObject.ItemTypeEnum.Thrown) // bypass .IsRangedWeapon for throwing in alt usage melee mode
        {
            isThrowingWeapon = true;
            return type == ItemObject.ItemTypeEnum.Thrown;
        }

        if (!agent.GetWieldedWeaponInfo(Agent.HandIndex.MainHand).IsRangedWeapon) // is ranged weapon
        {
            return false;
        }

        return type == ItemObject.ItemTypeEnum.Bow ||
                   type == ItemObject.ItemTypeEnum.Crossbow ||
                   type == ItemObject.ItemTypeEnum.Musket ||
                   type == ItemObject.ItemTypeEnum.Thrown;
    }

    public static bool IsQuiverAmmoWeaponForRangedWeapon(MissionWeapon ammo, MissionWeapon rangedWeapon)
    {
        if (ammo.IsEmpty || rangedWeapon.IsEmpty)
        {
            return false;
        }

        rangedWeapon.GatherInformationFromWeapon(
            out bool hasMelee, out bool hasShield, out bool hasPolearm,
            out bool hasNonConsumableRanged, out bool hasThrown,
            out WeaponClass expectedAmmoClass);

        // Thrown weapons
        if (hasThrown && ammo.Item != null && ammo.Item.ItemType == ItemObject.ItemTypeEnum.Thrown)
        {
            return true;
        }

        // Regular ranged weapons (bow, crossbow, musket)
        if (!ammo.IsEmpty && ammo.GetWeaponComponentDataForUsage(0)?.WeaponClass == expectedAmmoClass)
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
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsServer)
        {
            registerer.Register<AmmoQuiverChangeRequestClientMessage>((msg, peer) => HandleQuiverClientMessage(msg, peer));
        }
    }

    private bool HandleQuiverClientMessage(NetworkCommunicator peer, AmmoQuiverChangeRequestClientMessage message)
    {
        ResetReloadAnimationsAndWeapon(peer);
        ExecuteClientAmmoQuiverChange(peer);
        return true;
    }

    private void ResetReloadAnimationsAndWeapon(NetworkCommunicator peer)
    {
        Agent agent = peer.ControlledAgent;
        if (agent == null || !agent.IsActive())
        {
            return;
        }

        EquipmentIndex wieldedIndex = agent.GetPrimaryWieldedItemIndex();
        MissionWeapon wieldedWeapon = agent.WieldedWeapon;

        if (!wieldedWeapon.IsEmpty && !wieldedWeapon.IsEqualTo(MissionWeapon.Invalid) && wieldedIndex >= EquipmentIndex.Weapon0 && wieldedIndex <= EquipmentIndex.Weapon3)
        {
            // stops reload for bows and early stages of xbow/gun reload
            agent.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.Instant);
            agent.TryToWieldWeaponInSlot(wieldedIndex, Agent.WeaponWieldActionType.Instant, true);
        }
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

        // If there are more than 1 quivers, perform swaps
        if (ammoQuivers.Count < 2)
        {
            return;
        }

        // Verify ranged weapon wielded
        if (!IsAgentWieldedWeaponRangedUsesQuiver(agent, out EquipmentIndex wieldedWeaponIndex, out MissionWeapon wieldedWeapon, out bool isThrowingWeapon))
        {
            return;
        }

        // handle throwing
        if (isThrowingWeapon == true)
        {
            CycleThrowingQuivers(agent, wieldedWeaponIndex, equipment, ammoQuivers);
        }
        else // bow,xbow/musket
        {
            SwapQuivers(agent, equipment, ammoQuivers);
        }

        agent.UpdateWeapons();

        GameNetwork.BeginModuleEventAsServer(peer);
        GameNetwork.WriteMessage(new AmmoQuiverChangeSuccessServerMessage());
        GameNetwork.EndModuleEventAsServer();
    }

    private void CycleThrowingQuivers(Agent agent, EquipmentIndex wieldedWeaponIndex, MissionEquipment equipment, List<int> ammoQuivers)
    {
        if (agent == null || !agent.IsActive() || ammoQuivers == null || ammoQuivers.Count <= 1)
        {
            return;
        }

        int currentIndex = ammoQuivers.IndexOf((int)wieldedWeaponIndex);
        if (currentIndex == -1)
        {
            return; // current index isn't in the list
        }

        for (int i = 1; i < ammoQuivers.Count; i++)
        {
            int checkIndex = (currentIndex + i) % ammoQuivers.Count;
            var checkEquipmentIndex = (EquipmentIndex)ammoQuivers[checkIndex];
            var checkWeapon = equipment[checkEquipmentIndex];

            if (!checkWeapon.IsEmpty && checkWeapon.Amount > 0)
            {
                agent.TryToWieldWeaponInSlot(checkEquipmentIndex, Agent.WeaponWieldActionType.WithAnimation, false);
                return;
            }
        }
    }

    private void SwapQuivers(Agent agent, MissionEquipment equipment, System.Collections.Generic.List<int> ammoQuivers)
    {
        if (agent == null || !agent.IsActive() || ammoQuivers == null || ammoQuivers.Count < 2)
        {
            return;
        }

        int count = ammoQuivers.Count;

        // Cache the original MissionWeapons in order
        MissionWeapon[] cachedWeapons = new MissionWeapon[count];
        for (int i = 0; i < count; i++)
        {
            cachedWeapons[i] = equipment[ammoQuivers[i]];
        }

        // Rotate left: shift each weapon to the previous index
        for (int i = 0; i < count; i++)
        {
            int fromIndex = (i + 1) % count;
            agent.EquipWeaponWithNewEntity((EquipmentIndex)ammoQuivers[i], ref cachedWeapons[fromIndex]);
        }
    }
}

using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>
/// Server behavior that handles in-game equipment changes. Changes are in-memory only
/// and take effect on the next spawn. Not persisted to the WebAPI.
/// </summary>
internal class CrpgInventoryBehavior : MissionNetwork
{
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsServer)
        {
            registerer.Register<RequestEquipItem>(OnRequestEquipItem);
        }
    }

    private bool OnRequestEquipItem(NetworkCommunicator networkPeer, RequestEquipItem message)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        if (crpgPeer?.User == null)
        {
            return false;
        }

        var equippedItems = crpgPeer.User.Character.EquippedItems;
        var slot = message.Slot.ToSlot();

        if (message.Item == null)
        {
            for (int i = equippedItems.Count - 1; i >= 0; i--)
            {
                if (equippedItems[i].Slot == slot)
                {
                    equippedItems.RemoveAt(i);
                }
            }

            crpgPeer.SynchronizeUserToEveryone();
            return true;
        }

        var ownedItem = FindOwnedItem(crpgPeer.OwnedItems, message.Item);
        if (ownedItem == null)
        {
            Debug.Print($"Player tried to equip item '{message.Item.StringId}' they don't own");
            return false;
        }

        if (!Equipment.IsItemFitsToSlot(message.Slot, message.Item))
        {
            Debug.Print($"Item '{message.Item.StringId}' does not fit in slot {message.Slot}");
            return false;
        }

        // Remove any existing item in the target slot.
        for (int i = equippedItems.Count - 1; i >= 0; i--)
        {
            if (equippedItems[i].Slot == slot)
            {
                equippedItems.RemoveAt(i);
            }
        }

        equippedItems.Add(new CrpgEquippedItem
        {
            Slot = slot,
            UserItem = new CrpgUserItem
            {
                ItemId = ownedItem.Item.StringId,
                Rank = ownedItem.Rank,
                IsBroken = ownedItem.IsBroken,
            },
        });

        crpgPeer.SynchronizeUserToEveryone();
        return true;
    }

    private CrpgOwnedItem? FindOwnedItem(IList<CrpgOwnedItem> ownedItems, ItemObject item)
    {
        foreach (var ownedItem in ownedItems)
        {
            if (ownedItem.Item == item)
            {
                return ownedItem;
            }
        }

        return null;
    }
}

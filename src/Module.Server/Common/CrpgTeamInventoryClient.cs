using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network.TeamInventory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common;

internal class CrpgTeamInventoryClient : MissionNetwork
{
    internal const int TeamInventoryItemId = -2;
    public bool IsEnabled { get; private set; }
    public IList<CrpgTeamInventoryItem> TeamItems { get; private set; } = [];
    private readonly List<CrpgEquippedItemExtended> _equippedItems = [];
    public IReadOnlyList<CrpgEquippedItemExtended> EquippedItems => _equippedItems;
    public Dictionary<CrpgItemSlot, string> LastEquippedItems { get; private set; } = [];

    internal event Action? OnTeamItemsUpdated;
    internal event Action? OnEquippedTeamItemsUpdated;
    internal event Action<CrpgTeamInventoryItem>? OnSingleTeamItemQuantityUpdated;
    internal event Action<bool>? OnTeamInventoryEnabledChanged;
    internal event Action<bool, bool, string>? OnForceOpenEquipMenu;
    internal event Action<string, bool, float>? OnStatusMessageRequested;

    internal static bool IsTeamInventoryItem(int id) => id == TeamInventoryItemId;
    internal static int GetRankFromItemId(string id)
    {
        if (id.EndsWith("_h3"))
        {
            return 3;
        }

        if (id.EndsWith("_h2"))
        {
            return 2;
        }

        if (id.EndsWith("_h1"))
        {
            return 1;
        }

        return 0;
    }

    internal void RequestEquipTeamItem(CrpgItemSlot slot, string id, bool equip)
    {
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestEquipTeamItem
        {
            Slot = slot,
            Item = id,
            Equip = equip,
        });
        GameNetwork.EndModuleEventAsClient();
    }

    internal void RequestSpawnWithTeamEquipment()
    {
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestSpawnWithTeamEquipment());
        GameNetwork.EndModuleEventAsClient();
    }

    internal void RequestGetTeamItemsList()
    {
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestGetTeamItemsList());
        GameNetwork.EndModuleEventAsClient();
    }

    internal bool IsTeamItemEquipped(string itemId)
    {
        return EquippedItems.Any(e => e.UserItem.ItemId == itemId);
    }

    internal CrpgTeamInventoryItem? FindTeamInventoryItem(string id)
    {
        return TeamItems.FirstOrDefault(i => i.Id == id);
    }

    internal Equipment GetSelectedTeamInventoryEquipment()
    {
        var baseEquippedItems = EquippedItems
            .Select(e => new CrpgEquippedItem
            {
                Slot = e.Slot,
                UserItem = new CrpgUserItem
                {
                    Id = e.UserItem.Id,
                    ItemId = e.UserItem.ItemId,
                },
            })
            .ToList();
        return CrpgCharacterBuilder.CreateCharacterEquipment(baseEquippedItems);
    }

    internal List<string> GetUnavailableLastEquippedItems()
    {
        return LastEquippedItems.Values
            .Where(itemId =>
            {
                var item = FindTeamInventoryItem(itemId);
                return item == null || item.Quantity <= 0;
            })
            .Distinct() // same item could be in multiple slots
            .ToList();
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<ServerSendTeamItemsList>(HandleServerSendTeamItemsList);
        registerer.Register<ServerSendTeamItemQuantityUpdated>(HandleServerSendTeamItemQuantityUpdated);
        registerer.Register<ServerSendTeamItemsQuantityUpdated>(HandleServerSendTeamItemsQuantityUpdated);
        registerer.Register<ServerSendEquipTeamItemResult>(HandleServerSendEquipTeamItemResult);
        registerer.Register<ServerSendTeamInventoryEnabled>(HandleServerSendTeamInventoryEnabled);
        registerer.Register<ServerSendLastUsedEquipment>(HandleServerSendLastUsedEquipment);
        registerer.Register<ServerSendForceEquipMenu>(HandleServerSendForceEquipMenu);
    }

    private void HandleServerSendTeamItemsList(ServerSendTeamItemsList message)
    {
        TeamItems = message.Items;
        OnTeamItemsUpdated?.Invoke();
    }

    private void HandleServerSendTeamItemQuantityUpdated(ServerSendTeamItemQuantityUpdated message)
    {
        UpdateItemQuantity(message.Item, message.Quantity);
    }

    private void HandleServerSendTeamItemsQuantityUpdated(ServerSendTeamItemsQuantityUpdated message)
    {
        foreach (var kvp in message.Items)
        {
            UpdateItemQuantity(kvp.Key, kvp.Value);
        }
    }

    private void HandleServerSendEquipTeamItemResult(ServerSendEquipTeamItemResult message)
    {
        _equippedItems.RemoveAll(e => e.Slot == message.Slot);

        if (message.Equipped)
        {
            _equippedItems.Add(new CrpgEquippedItemExtended
            {
                Slot = message.Slot,
                UserItem = new CrpgUserItemExtended { Id = TeamInventoryItemId, ItemId = message.Item },
            });
        }

        var itemObj = MBObjectManager.Instance.GetObject<ItemObject>(message.Item);
        string itemName = itemObj?.Name?.ToString() ?? "Unknown";

        OnStatusMessageRequested?.Invoke(
            message.Equipped
                ? new TextObject("{=KC9dx228}Equipped {ITEM_NAME} in {SLOT}.")
                    .SetTextVariable("ITEM_NAME", itemName)
                    .SetTextVariable("SLOT", message.Slot.ToString())
                    .ToString()
                : new TextObject("{=KC9dx229}Unequipped {ITEM_NAME} from {SLOT}.")
                    .SetTextVariable("ITEM_NAME", itemName)
                    .SetTextVariable("SLOT", message.Slot.ToString())
                    .ToString(),
            false,
            5f);

        UpdateItemQuantity(message.Item, message.Quantity);
        OnEquippedTeamItemsUpdated?.Invoke();
    }

    private void HandleServerSendTeamInventoryEnabled(ServerSendTeamInventoryEnabled message)
    {
        IsEnabled = message.IsEnabled;
        OnTeamInventoryEnabledChanged?.Invoke(message.IsEnabled);
    }

    private void HandleServerSendLastUsedEquipment(ServerSendLastUsedEquipment message)
    {
        LastEquippedItems = message.Equipment;
    }

    private void HandleServerSendForceEquipMenu(ServerSendForceEquipMenu message)
    {
        if (IsEnabled)
        {
            OnForceOpenEquipMenu?.Invoke(message.Show, message.ShowReadyButton, message.Msg);
        }
    }

    private void UpdateItemQuantity(string itemId, int quantity)
    {
        var item = TeamItems.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            item.Quantity = quantity;
            OnSingleTeamItemQuantityUpdated?.Invoke(item);
        }
    }
}

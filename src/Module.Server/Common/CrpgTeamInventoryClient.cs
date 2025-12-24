using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network.TeamInventory;
using Crpg.Module.GUI.Inventory;
using Crpg.Module.GUI.Notifications;
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
    private readonly List<byte> _teamItemsChunkBuffer = [];
    public IReadOnlyList<CrpgEquippedItemExtended> EquippedItems => _equippedItems;
    public Dictionary<CrpgItemSlot, string> LastEquippedItems { get; private set; } = [];

    internal event Action? OnTeamItemsUpdated;
    internal event Action? OnEquippedTeamItemsUpdated;
    internal event Action<CrpgTeamInventoryItem>? OnSingleTeamItemQuantityUpdated;
    internal event Action<bool>? OnTeamInventoryEnabledChanged;
    internal event Action<bool, bool, string>? OnForceOpenEquipMenu;

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

    /// <summary>
    /// Returns items from LastEquippedItems whose pool quantity has dropped to zero
    /// and that are not currently in EquippedItems. The exclusion of currently-equipped
    /// items prevents false positives when the player holds the last copy of something.
    /// </summary>
    internal List<string> GetUnavailableLastEquippedItems()
    {
        var currentlyEquipped = EquippedItems
            .Select(e => e.UserItem.ItemId)
            .Where(id => !string.IsNullOrEmpty(id))
            .ToHashSet();

        return LastEquippedItems.Values
            .Where(itemId =>
            {
                if (currentlyEquipped.Contains(itemId))
                {
                    return false; // successfully re-equipped, don't report
                }

                var item = FindTeamInventoryItem(itemId);
                return item == null || item.Quantity <= 0;
            })
            .Distinct()
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

    private void UpdateItemQuantity(string itemId, int quantity)
    {
        var item = TeamItems.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            item.Quantity = quantity;
            OnSingleTeamItemQuantityUpdated?.Invoke(item);
        }
    }

    private void HandleServerSendTeamItemsList(ServerSendTeamItemsList message)
    {
        if (message.IsFirstChunk)
        {
            _teamItemsChunkBuffer.Clear();
        }

        _teamItemsChunkBuffer.AddRange(message.ChunkData);

        if (!message.IsLastChunk)
        {
            return;
        }

        try
        {
            using System.IO.MemoryStream compressed = new(_teamItemsChunkBuffer.ToArray(), writable: false);
            using System.IO.Compression.GZipStream gzip = new(compressed, System.IO.Compression.CompressionMode.Decompress);
            using System.IO.BinaryReader reader = new(gzip);

            int count = reader.ReadInt32();
            var items = new List<CrpgTeamInventoryItem>(count);
            for (int i = 0; i < count; i++)
            {
                items.Add(new CrpgTeamInventoryItem
                {
                    Id = reader.ReadString(),
                    Quantity = reader.ReadInt32(),
                    Restricted = reader.ReadBoolean(),
                });
            }

            TeamItems = items;
            OnTeamItemsUpdated?.Invoke();
        }
        catch (Exception e)
        {
            Debug.Print($"[TeamInventoryClient] Team items decompression failed: {e.Message}", 0, Debug.DebugColor.Red);
        }
        finally
        {
            _teamItemsChunkBuffer.Clear();
        }
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

        ShowStatusLog(
            message.Equipped
                ? new TextObject("{=KC9dx228}Equipped {ITEM_NAME} in {SLOT}.")
                    .SetTextVariable("ITEM_NAME", itemName)
                    .SetTextVariable("SLOT", EquipmentSlotVM.GetFriendlySlotName(message.Slot))
                    .ToString()
                : new TextObject("{=KC9dx229}Unequipped {ITEM_NAME} from {SLOT}.")
                    .SetTextVariable("ITEM_NAME", itemName)
                    .SetTextVariable("SLOT", EquipmentSlotVM.GetFriendlySlotName(message.Slot))
                    .ToString(),
            message.Equipped ? new Color(0.4f, 1f, 0.4f, 1f) : new Color(1f, 0.8f, 0.2f, 1f));

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

    private static void ShowStatusLog(string text, Color color, float duration = 5f)
    {
        CrpgHudNotificationUiHandler.AddToLog("statuslog", new CrpgHudNotificationOptions
        {
            Text = text,
            Color = color,
            Duration = duration,
        });
    }
}

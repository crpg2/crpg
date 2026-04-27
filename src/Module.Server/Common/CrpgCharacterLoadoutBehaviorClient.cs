using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network.Armory;
using Crpg.Module.Common.Network.CharacterLoadout;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common;

internal class CrpgCharacterLoadoutBehaviorClient : MissionNetwork
{
    private readonly List<CrpgEquippedItemExtended> _equippedItems = [];
    private readonly List<CrpgUserItemExtended> _userInventoryItems = [];
    private CrpgClanArmoryClient? _clanArmory;
    private CrpgTeamInventoryClient? _teamInventory;
    public bool IsReadyToSpawn { get; internal set; } = true;
    public bool IsEnabled { get; private set; } = false;

    // Public read-only access
    public IReadOnlyList<CrpgEquippedItemExtended> EquippedItems => _equippedItems;
    public IReadOnlyList<CrpgUserItemExtended> UserInventoryItems => _userInventoryItems;

    public CrpgUser User { get; private set; } = new(); // will only contain some of CrpgUser data
    public CrpgCharacter UserCharacter { get; private set; } = new();

    internal event Action<bool>? OnUserCharacterLoadoutEnabledChanged;
    internal event Action? OnUserInventoryUpdated;
    internal event Action? OnUserInfoUpdated;
    internal event Action? OnUserCharacterEquippedItemsUpdated;
    internal event Action? OnUserCharacterBasicUpdated;
    internal event Action? OnUserCharacteristicsUpdated;
    internal event Action? OnUserCharacteristicsConverted;
    internal event Action<CrpgItemSlot>? OnEquipmentSlotUpdated;
    internal event Action<bool, bool, string>? OnForceOpenEquipMenu;
    internal event Action<string, bool, float>? OnStatusMessageRequested;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();
        if (_clanArmory is not null)
        {
            _clanArmory.OnArmoryActionUpdated += HandleArmoryActionUpdated;
        }

        _teamInventory = Mission.Current?.GetMissionBehavior<CrpgTeamInventoryClient>();
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();

        if (_clanArmory is not null)
        {
            _clanArmory.OnArmoryActionUpdated -= HandleArmoryActionUpdated;
        }
    }

    internal void RequestSetSpawnReady()
    {
        IsReadyToSpawn = true;
        SendMessageToServer(new UserRequestReadyToSpawn());
    }

    internal void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
        OnUserCharacterLoadoutEnabledChanged?.Invoke(enabled);
    }

    // ===== API METHODS =====
    internal void RequestGetUserInfo() => SendMessageToServer(new UserRequestGetUserInfo());
    internal void RequestGetUserInventoryItems() => SendMessageToServer(new UserRequestGetInventoryItems());
    internal void RequestGetUpdatedCharacterEquippedItems() => SendMessageToServer(new UserRequestGetEquippedItems());
    internal void RequestGetUpdatedCharacterBasic() => SendMessageToServer(new UserRequestGetCharacterBasic());

    internal void RequestUpdateCharacterCharacteristics(CrpgCharacterCharacteristics characteristics) =>
        SendMessageToServer(new UserRequestUpdateCharacterCharacteristics { Characteristics = characteristics });

    internal void RequestConvertCharacterCharacteristic(CrpgGameCharacteristicConversionRequest conversionType) =>
        SendMessageToServer(new UserRequestConvertCharacteristics { ConversionRequest = conversionType });

    internal Equipment GetCrpgUserCharacterEquipment()
    {
        // Convert extended equipped items to base CrpgEquippedItem
        var baseEquippedItems = EquippedItems
            .Select(e => new CrpgEquippedItem
            {
                Slot = e.Slot,
                UserItem = new CrpgUserItem { Id = e.UserItem.Id, ItemId = e.UserItem.ItemId },
            })
            .ToList();
        return CrpgCharacterBuilder.CreateCharacterEquipment(baseEquippedItems);
    }

    internal bool IsItemEquipped(int userItemId) => EquippedItems.Any(e => e.UserItem.Id == userItemId);

    internal CrpgUserItemExtended? GetCrpgUserItem(int uItemId)
    {
        return _userInventoryItems.FirstOrDefault(i => i.Id == uItemId)
            ?? _clanArmory?.FindArmoryItem(uItemId)?.UserItem;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<ServerSendUserInventoryItems>(HandleUpdateCrpgUserInventory); // recieve user items from server
        registerer.Register<ServerSendUserCharacterEquippedItems>(HandleUpdateCrpgCharacterEquippedItems); // recieve equipped items for character from server
        registerer.Register<ServerSendEquipItemResult>(HandleEquipItemResult); // recieve result of attempt to equip item in slot on api from server
        registerer.Register<ServerSendUserCharacterBasic>(HandleUpdateCrpgUserCharacterBasic); // recieve character basic from server
        registerer.Register<ServerSendUpdateCharacteristicsResult>(HandleUpdateCharacteristicsResult); // recieve result of attempt to update character characteristics on api from server
        registerer.Register<ServerSendConvertCharacteristicsResult>(HandleConvertCharacteristicsResult); // recieve result of attempt to convert character characteristics on api from server
        registerer.Register<ServerSendUserInfo>(HandleUpdateUserInfo); // recieve user info from server
        registerer.Register<ServerSendUserCharacterLoadoutEnabled>(HandleServerSendUserCharacterLoadoutEnabled);
        registerer.Register<ServerSendForceEquipMenu>(HandleServerSendForceEquipMenu);
    }

    private void SetEquippedItems(IEnumerable<CrpgEquippedItemExtended> items)
    {
        var newItems = items.ToList();

        bool changed = newItems.Count != _equippedItems.Count ||
            newItems.Any(n => !_equippedItems.Any(e => e.Slot == n.Slot && e.UserItem?.Id == n.UserItem?.Id));

        _equippedItems.Clear();
        _equippedItems.AddRange(newItems);

        if (changed)
        {
            OnStatusMessageRequested?.Invoke(new TextObject("{=KC9dx216}Equipment updated.").ToString(), false, 5f);
        }
    }

    private void SetEquippedItem(CrpgItemSlot slot, CrpgUserItemExtended? userItem)
    {
        var currentEquippedItem = _equippedItems.FirstOrDefault(e => e.Slot == slot);
        if (userItem == null)
        {
            if (currentEquippedItem != null)
            {
                _equippedItems.Remove(currentEquippedItem);
                ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(currentEquippedItem.UserItem.ItemId ?? string.Empty);
                string itemName = item?.Name?.ToString() ?? string.Empty;
                OnStatusMessageRequested?.Invoke(new TextObject("{=KC9dx217}Unequipped {ITEM_NAME} from {SLOT}.")
                    .SetTextVariable("ITEM_NAME", itemName)
                    .SetTextVariable("SLOT", slot.ToString()).ToString(), false, 5f);
            }
        }
        else
        {
            var newEquippedItem = new CrpgEquippedItemExtended { Slot = slot, UserItem = userItem };
            if (currentEquippedItem == null)
            {
                _equippedItems.Add(newEquippedItem);
            }
            else
            {
                _equippedItems[_equippedItems.IndexOf(currentEquippedItem)] = newEquippedItem;
            }

            ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(newEquippedItem.UserItem.ItemId ?? string.Empty);
            string itemName = item?.Name?.ToString() ?? string.Empty;
            OnStatusMessageRequested?.Invoke(new TextObject("{=KC9dx218}Equipped {ITEM_NAME} to {SLOT}.")
                .SetTextVariable("ITEM_NAME", itemName)
                .SetTextVariable("SLOT", slot.ToString()).ToString(), false, 5f);
        }

        OnEquipmentSlotUpdated?.Invoke(slot); // notify VM of only this slot
        SoundEvent.PlaySound2D("event:/ui/inventory/pickup");
    }

    private void RemoveEquippedItem(int userItemId)
    {
        // Find all equipped items matching the userItemId
        var itemsToRemove = _equippedItems.Where(e => e.UserItem.Id == userItemId).ToList();

        foreach (var equippedItem in itemsToRemove)
        {
            _equippedItems.Remove(equippedItem);
            OnEquipmentSlotUpdated?.Invoke(equippedItem.Slot); // Trigger the slot updated event for UI to refresh
            ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(equippedItem.UserItem.ItemId ?? string.Empty);
            string itemName = item?.Name?.ToString() ?? string.Empty;
            OnStatusMessageRequested?.Invoke(new TextObject("{=KC9dx217}Unequipped {ITEM_NAME} from {SLOT}.")
                .SetTextVariable("ITEM_NAME", itemName)
                .SetTextVariable("SLOT", equippedItem.Slot.ToString()).ToString(), false, 5f); // reuse KC9dx217
        }

        if (itemsToRemove.Count > 0)
        {
            SoundEvent.PlaySound2D("event:/ui/inventory/pickup");
        }
    }

    private void SetUserInventoryItems(IEnumerable<CrpgUserItemExtended> items)
    {
        _userInventoryItems.Clear();
        _userInventoryItems.AddRange(items);
    }

    private void SetUserCharacterBasic(CrpgCharacter crpgCharacter) => UserCharacter = crpgCharacter;

    private void HandleUpdateUserInfo(ServerSendUserInfo message)
    {
        if (message?.User?.Character == null)
        {
            return;
        }

        User ??= new CrpgUser();
        User.Platform = message.User.Platform;
        User.PlatformUserId = message.User.PlatformUserId;
        User.Region = message.User.Region;
        User.Id = message.User.Id;
        User.Name = message.User.Name;
        User.Gold = message.User.Gold;
        User.Character ??= new CrpgCharacter();
        User.Character.Name = message.User.Character.Name;
        User.Character.Generation = message.User.Character.Generation;
        User.Character.Level = message.User.Character.Level;
        User.Character.Experience = message.User.Character.Experience;
        User.ClanMembership ??= new CrpgClanMember();
        User.ClanMembership = message.User.ClanMembership;

        // User = message.User;
        OnUserInfoUpdated?.Invoke();
    }

    private void SendMessageToServer(GameNetworkMessage message)
    {
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(message);
        GameNetwork.EndModuleEventAsClient();
    }

    private void HandleArmoryActionUpdated(ClanArmoryActionType action, int uItemId)
    {
        if (action == ClanArmoryActionType.Get)
        {
            return;
        }

        CrpgClanArmoryItem? clanArmoryItem = _clanArmory?.FindArmoryItem(uItemId);
        CrpgUserItemExtended? userInventoryItem = _userInventoryItems.FirstOrDefault(i => i.Id == uItemId);
        string? itemId = userInventoryItem?.ItemId ?? clanArmoryItem?.UserItem?.ItemId;
        var itemObj = itemId != null ? MBObjectManager.Instance.GetObject<ItemObject>(itemId) : null;
        string itemName = itemObj?.Name?.ToString() ?? uItemId.ToString();
        string strText = string.Empty;

        switch (action)
        {
            case ClanArmoryActionType.Add:
                if (clanArmoryItem?.UserItem != null) // Already in the list, just update flags
                {
                    clanArmoryItem.UserItem.IsArmoryItem = true;
                    if (userInventoryItem != null)
                    {
                        RemoveEquippedItem(userInventoryItem.Id);
                        userInventoryItem.IsArmoryItem = true;
                        strText = new TextObject("{=KC9dx219}{ITEM_NAME} added to clan armory.")
                            .SetTextVariable("ITEM_NAME", itemName).ToString();
                    }
                }

                break;

            case ClanArmoryActionType.Remove:
                if (userInventoryItem != null)
                {
                    userInventoryItem.IsArmoryItem = false;
                    strText = new TextObject("{=KC9dx220}{ITEM_NAME} removed from clan armory.")
                        .SetTextVariable("ITEM_NAME", itemName).ToString();
                }

                break;

            case ClanArmoryActionType.Borrow:
                if (clanArmoryItem?.UserItem != null)
                {
                    clanArmoryItem.UserItem.IsArmoryItem = true;
                    clanArmoryItem.BorrowedItem = new CrpgClanArmoryBorrowedItem
                    {
                        BorrowerUserId = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.GetComponent<CrpgPeer>()?.User?.Id ?? 0,
                        UserItemId = clanArmoryItem.UserItem.Id,
                        UpdatedAt = DateTime.UtcNow,
                    };

                    if (userInventoryItem != null)
                    {
                        userInventoryItem.IsArmoryItem = true;
                    }
                    else
                    { // Add the borrowed item to user inventory
                        _userInventoryItems.Add(clanArmoryItem.UserItem);
                    }

                    strText = new TextObject("{=KC9dx221}{ITEM_NAME} borrowed from clan armory.")
                        .SetTextVariable("ITEM_NAME", itemName).ToString();
                }

                break;

            case ClanArmoryActionType.Return:
                if (clanArmoryItem?.UserItem != null)
                { // remove if equipped
                    RemoveEquippedItem(clanArmoryItem.UserItem.Id);
                    clanArmoryItem.UserItem.IsArmoryItem = true;
                    clanArmoryItem.BorrowedItem = null;
                    if (userInventoryItem != null)
                    {
                        userInventoryItem.IsArmoryItem = true;
                        _userInventoryItems.Remove(userInventoryItem);
                    }

                    strText = new TextObject("{=KC9dx222}{ITEM_NAME} returned to clan armory.")
                        .SetTextVariable("ITEM_NAME", itemName).ToString();
                }

                break;
            default:
                OnStatusMessageRequested?.Invoke(new TextObject("{=KC9dx223}ClanArmory: unknown action {ACTION}")
                    .SetTextVariable("ACTION", action.ToString()).ToString(), true, 5f);

                return;
        }

        OnStatusMessageRequested?.Invoke($"{strText}", false, 5f);
        SoundEvent.PlaySound2D("event:/ui/inventory/pickup");
    }

    /// <summary>
    /// Handles a message from the server containing the user's equipped items.
    /// Updates local state and triggers <see cref="OnUserCharacterEquippedItemsUpdated"/>.
    /// </summary>
    /// <param name="message">The server message containing equipped items.</param>
    private void HandleUpdateCrpgCharacterEquippedItems(ServerSendUserCharacterEquippedItems message)
    {
        if (message.Items == null)
        {
            Debug.Print($"Error in HandleUpdateCrpgCharacterEquippedItems: message.Items was null");
            return;
        }

        SetEquippedItems(message.Items);
        OnUserCharacterEquippedItemsUpdated?.Invoke();
    }

    /// <summary>
    /// Handles a message from the server containing the user's inventory items.
    /// Updates local state and triggers <see cref="OnUserInventoryUpdated"/>.
    /// </summary>
    /// <param name="message">The server message containing inventory items.</param>
    private void HandleUpdateCrpgUserInventory(ServerSendUserInventoryItems message)
    {
        if (message.Items == null)
        {
            Debug.Print($"Error in HandleUpdateCrpgUserInventory: message.Items was null");
            return;
        }

        SetUserInventoryItems(message.Items);
        OnUserInventoryUpdated?.Invoke();
    }

    private void HandleUpdateCrpgUserCharacterBasic(ServerSendUserCharacterBasic message)
    {
        if (message.Character == null)
        {
            Debug.Print("Error in HandleUpdateCrpgUserCharacterBasic: message.Character was null");
            return;
        }

        SetUserCharacterBasic(message.Character);
        OnUserCharacterBasicUpdated?.Invoke();
    }

    /// <summary>
    /// Handles a message from the server confirming the result of an equip/unequip action.
    /// Updates the affected slot in local equipped items state.
    /// </summary>
    /// <param name="message">The server message containing the equip result.</param>
    private void HandleEquipItemResult(ServerSendEquipItemResult message)
    {
        if (!message.Success)
        {
            Debug.Print($"Equip failed: {message.ErrorMessage}");
            return;
        }

        var slot = (CrpgItemSlot)message.SlotIndex;

        CrpgUserItemExtended? userItem = null;

        if (message.UserItemId != -1)
        {
            // First try to find in user inventory
            userItem = _userInventoryItems.FirstOrDefault(i => i.Id == message.UserItemId);
            userItem ??= _clanArmory?.FindArmoryItem(message.UserItemId)?.UserItem; // If not found, maybe it’s a borrowed armory item
        }

        SetEquippedItem(slot, userItem);
    }

    private void HandleUpdateCharacteristicsResult(ServerSendUpdateCharacteristicsResult message)
    {
        if (!message.Success || message.Characteristics == null)
        {
            Debug.Print($"Update Characteristics Failed: {message.ErrorMessage}");
            OnStatusMessageRequested?.Invoke(new TextObject("{=KC9dx224}Updated Characteristics Failed: {ERROR}")
                .SetTextVariable("ERROR", message.ErrorMessage).ToString(), true, 5f);
            return;
        }

        UserCharacter.Characteristics = message.Characteristics;
        UISoundsHelper.PlayUISound("event:/ui/panels/upgrade");
        OnUserCharacteristicsUpdated?.Invoke();
        OnStatusMessageRequested?.Invoke(
            new TextObject("{=KC9dx225}Updated Characteristics Successfully").ToString(), false, 5f);
    }

    private void HandleConvertCharacteristicsResult(ServerSendConvertCharacteristicsResult message)
    {
        if (!message.Success || message.AttributesPoints < 0 || message.SkillPoints < 0)
        {
            Debug.Print($"Convert Characteristics Failed: {message.ErrorMessage}");
            return;
        }

        UserCharacter.Characteristics.Attributes.Points = message.AttributesPoints;
        UserCharacter.Characteristics.Skills.Points = message.SkillPoints;
        OnUserCharacteristicsConverted?.Invoke();
    }

    private void HandleServerSendUserCharacterLoadoutEnabled(ServerSendUserCharacterLoadoutEnabled message)
    {
        SetEnabled(message.IsEnabled);
    }

    private void HandleServerSendForceEquipMenu(ServerSendForceEquipMenu message)
    {
        if (_teamInventory?.IsEnabled != true)
        {
            IsReadyToSpawn = false;
            OnForceOpenEquipMenu?.Invoke(message.Show, message.ShowReadyButton, message.Msg);
        }
    }
}

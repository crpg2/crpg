using System.Collections;
using Crpg.Module.Api;
using Crpg.Module.Api.Exceptions;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network;
using Crpg.Module.Common.Network.Armory;
using Crpg.Module.GUI.Inventory;
using Messages.FromClient.ToLobbyServer;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;

internal class CrpgCharacterLoadoutBehaviorClient : MissionNetwork
{
    private readonly List<CrpgEquippedItemExtended> _equippedItems = new();
    private readonly List<CrpgUserItemExtended> _userInventoryItems = new();

    private CrpgClanArmoryClient? _clanArmory;

    // private readonly List<CrpgClanArmoryItem> _clanArmoryItems = new();
    private MissionNetworkComponent? _missionNetworkComponent;

    // Public read-only access
    public IReadOnlyList<CrpgEquippedItemExtended> EquippedItems => _equippedItems;
    public IReadOnlyList<CrpgUserItemExtended> UserInventoryItems => _userInventoryItems;
    // public IReadOnlyList<CrpgClanArmoryItem> ClanArmoryItems => _clanArmoryItems;

    public CrpgCharacter UserCharacter { get; private set; } = new();
    public CrpgCharacterStatistics UserCharacterStatistics { get; private set; } = new();
    public CrpgConstants Constants { get; }

    internal event Action? OnUserInventoryUpdated;
    internal event Action? OnClanArmoryUpdated;
    internal event Action? OnUserCharacterEquippedItemsUpdated;
    internal event Action? OnUserCharacterBasicUpdated;
    internal event Action? OnUserCharacteristicsUpdated;
    internal event Action? OnUserCharacteristicsConverted;
    internal event Action<CrpgItemSlot>? OnEquipmentSlotUpdated;
    internal event Action<ClanArmoryActionType, int>? OnArmoryActionUpdated;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _missionNetworkComponent = Mission.GetMissionBehavior<MissionNetworkComponent>();
        if (_missionNetworkComponent != null)
        {
            _missionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
        }

        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();
        if (_clanArmory is not null)
        {
            _clanArmory.OnArmoryActionUpdated += HandleArmoryActionUpdated;
        }
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        if (_missionNetworkComponent != null)
        {
            _missionNetworkComponent.OnMyClientSynchronized -= OnMyClientSynchronized;
        }

        if (_clanArmory is not null)
        {
            _clanArmory.OnArmoryActionUpdated -= HandleArmoryActionUpdated;
        }
    }

    public CrpgCharacterLoadoutBehaviorClient(CrpgConstants constants)
    {
        Constants = constants;
    }

    /// <summary>
    /// Builds a TaleWorlds Equipment object from the current equipped items.
    /// </summary>
    /// <returns>An <see cref="Equipment"/> object representing the user's equipped items.</returns>
    public Equipment GetCrpgUserCharacterEquipment()
    {
        // Convert extended equipped items to base CrpgEquippedItem
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

        // Build Equipment using CrpgCharacterBuilder
        return CrpgCharacterBuilder.CreateCharacterEquipment(baseEquippedItems);
    }

    /// <summary>
    /// Requests the latest user inventory and equipped items from the server.
    /// </summary>
    internal void RequestGetUpdatedEquipmentAndItems()
    {
        // Inventory items
        RequestGetUserInventoryItems();

        // Equipped items
        RequestGetUpdatedCharacterEquippedItems();

        // Character basic
        RequestGetUpdatedCharacterBasic();

        // Armory items
        // RequestArmoryAction(ClanArmoryActionType.Get, null);
        _clanArmory?.RequestArmoryAction(ClanArmoryActionType.Get, null);
    }

    internal void RequestGetUserInventoryItems()
    {
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestGetInventoryItems());
        GameNetwork.EndModuleEventAsClient();
    }

    internal void RequestGetUpdatedCharacterEquippedItems()
    {
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestGetEquippedItems());
        GameNetwork.EndModuleEventAsClient();
    }

    internal void RequestGetUpdatedCharacterBasic()
    {
        // Character basic
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestGetCharacterBasic());
        GameNetwork.EndModuleEventAsClient();
    }

    internal void RequestUpdateCharacterCharacteristics(CrpgCharacterCharacteristics characteristics)
    {
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestUpdateCharacterCharacteristics
        {
            Characteristics = characteristics,
        });
        GameNetwork.EndModuleEventAsClient();
    }

    internal void RequestConvertCharacterCharacteristic(CrpgGameCharacteristicConversionRequest conversionType)
    {
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestConvertCharacteristics
        {
            ConversionRequest = conversionType,
        });
        GameNetwork.EndModuleEventAsClient();
    }

    /// <summary>
    /// Replaces the current list of equipped items with the given items.
    /// </summary>
    /// <param name="items">The new equipped items to set.</param>
    internal void SetEquippedItems(IEnumerable<CrpgEquippedItemExtended> items)
    {
        _equippedItems.Clear();
        _equippedItems.AddRange(items);
    }

    /// <summary>
    /// Sets a single equipped item in the specified slot.
    /// Updates the list and triggers the <see cref="OnEquipmentSlotUpdated"/> event.
    /// </summary>
    /// <param name="slot">The equipment slot to update.</param>
    /// <param name="userItem">The item to equip, or null to unequip.</param>
    internal void SetEquippedItem(CrpgItemSlot slot, CrpgUserItemExtended? userItem)
    {
        var existing = _equippedItems.FirstOrDefault(e => e.Slot == slot);

        if (userItem == null)
        {
            // Unequip
            if (existing != null)
            {
                _equippedItems.Remove(existing);
            }
        }
        else
        {
            var newEq = new CrpgEquippedItemExtended
            {
                Slot = slot,
                UserItem = userItem,
            };

            if (existing == null)
            {
                _equippedItems.Add(newEq);
            }
            else
            {
                _equippedItems[_equippedItems.IndexOf(existing)] = newEq;
            }
        }

        OnEquipmentSlotUpdated?.Invoke(slot); // notify VM of only this slot
        UISoundsHelper.PlayUISound("event:/ui/transfer");
    }

    internal void RemoveEquippedItem(int userItemId)
    {
        // Find all equipped items matching the userItemId
        var itemsToRemove = _equippedItems.Where(e => e.UserItem.Id == userItemId).ToList();

        foreach (var equippedItem in itemsToRemove)
        {
            // Remove from the equipped items list
            _equippedItems.Remove(equippedItem);

            // Trigger the slot updated event for UI to refresh
            OnEquipmentSlotUpdated?.Invoke(equippedItem.Slot);

            UISoundsHelper.PlayUISound("event:/ui/transfer"); // optional feedback
        }
    }

    internal bool IsItemEquipped(int userItemId)
    {
        return EquippedItems.Any(e => e.UserItem.Id == userItemId);
    }

    /// <summary>
    /// Replaces the current list of user inventory items with the given items.
    /// </summary>
    /// <param name="items">The inventory items to set.</param>
    internal void SetUserInventoryItems(IEnumerable<CrpgUserItemExtended> items)
    {
        _userInventoryItems.Clear();
        _userInventoryItems.AddRange(items);
    }

    internal CrpgUserItemExtended? FindUserInventoryItemByUserItemId(int uItemId)
    {
        return UserInventoryItems.FirstOrDefaultQ(item =>
            item.Id >= 0 && item.Id == uItemId);
    }

    /*
   internal void SetClanArmoryItems(IEnumerable<CrpgClanArmoryItem> armoryItems)
       {
           _clanArmoryItems.Clear();
           _clanArmoryItems.AddRange(armoryItems);
       }
   */

    internal CrpgUserItemExtended? GetCrpgUserItem(int uItemId)
    {
        // First check the user’s inventory
        var userItem = UserInventoryItems
            .FirstOrDefault(i => i.Id == uItemId);

        if (userItem != null)
        {
            return userItem;
        }

        var armoryItem = _clanArmory?.FindClanArmoryItemByUserItemId(uItemId);

        // Then check the clan armory
        //        var armoryItem = _clanArmoryItems
        //          .FirstOrDefault(a => a.UserItem?.Id == uItemId);

        return armoryItem?.UserItem;
    }

    internal CrpgUserItemExtended? GetCrpgUserItem(CrpgUserItemExtended userItem)
    {
        if (userItem == null)
        {
            return null;
        }

        // First check if it's in the user's inventory
        var fromInventory = UserInventoryItems
            .FirstOrDefault(i => i.Id == userItem.Id);
        if (fromInventory != null)
        {
            return fromInventory;
        }

        // Then check the clan armory
        var fromArmory = _clanArmory?.FindClanArmoryItemByUserItemId(userItem.Id);
        return fromArmory?.UserItem;
    }


    /*
         internal CrpgClanArmoryItem? FindClanArmoryItemByUserItemId(int uItemId)
         {
             // Defensive: return null if no armory items exist
             if (_clanArmoryItems == null || _clanArmoryItems.Count == 0)
             {
                 LogDebugError("Error No clan armoy items exist");
                 return null;
             }

             foreach (var item in _clanArmoryItems)
             {
                 // Skip invalid entries
                 if (item == null)
                 {
                     LogDebugError("invalid entry");
                     continue;
                 }

                 // Skip if this armory item has no UserItem assigned
                 if (item.UserItem == null)
                 {
                     LogDebugError("Skipping armory item,  No userItem assigned");
                     continue;
                 }

                 // Check if IDs match
                 if (item.UserItem.Id == uItemId)
                 {
                     return item; // Found a match
                 }
             }

             // No match found
             LogDebugError("No Match Found");
             return null;
         }

     internal CrpgClanArmoryItem? FindClanArmoryItemByBorrowedUserItemId(int uItemId)
     {
         return _clanArmoryItems.FirstOrDefault(item =>
             item.BorrowedItem != null && item.BorrowedItem.UserItemId == uItemId);
     }
 */
    internal void SetUserCharacterBasic(CrpgCharacter crpgCharacter)
    {
        UserCharacter = crpgCharacter;
    }

    internal void SetUserCharacterStatistics(CrpgCharacterStatistics characteristics)
    {
        UserCharacterStatistics = characteristics;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<ServerSendUserInventoryItems>(HandleUpdateCrpgUserInventory); // recieve user items from server
        registerer.Register<ServerSendUserCharacterEquippedItems>(HandleUpdateCrpgCharacterEquippedItems); // recieve equipped items for character from server
        registerer.Register<ServerSendEquipItemResult>(HandleEquipItemResult); // recieve result of attempt to equip item in slot on api from server
        registerer.Register<ServerSendUserCharacterBasic>(HandleUpdateCrpgUserCharacterBasic); // recieve character basic from server
        registerer.Register<ServerSendUpdateCharacteristicsResult>(HandleUpdateCharacteristicsResult); // recieve result of attempt to update character characteristics on api from server
        registerer.Register<ServerSendConvertCharacteristicsResult>(HandleConvertCharacteristicsResult);
        registerer.Register<ServerSendUserCharacterStatistics>(HandleUpdateCharacterStatistics);
        // registerer.Register<ServerSendArmoryActionResult>(HandleRecieveArmoryActionResult);
    }

    private void OnMyClientSynchronized()
    {
        LogDebug("OnMyClientSynchronized:");
        RequestGetUpdatedEquipmentAndItems();
    }

    private void HandleArmoryActionUpdated(ClanArmoryActionType action, int uItemId)
    {
        CrpgClanArmoryItem? clanArmoryItem = null;
        CrpgUserItemExtended? userInventoryItem = null;

        if (action != ClanArmoryActionType.Get)
        {
            clanArmoryItem = _clanArmory?.FindClanArmoryItemByUserItemId(uItemId);
            userInventoryItem = FindUserInventoryItemByUserItemId(uItemId);
        }

        switch (action)
        {
            case ClanArmoryActionType.Add:
                if (clanArmoryItem?.UserItem != null)
                {
                    // Already in the list, just update flags
                    clanArmoryItem.UserItem.IsArmoryItem = true;
                    if (userInventoryItem != null)
                    {
                        userInventoryItem.IsArmoryItem = true;
                    }

                    LogDebugError($"userItem: {uItemId} already exists in clan armory, updated flags.");
                }

                if (userInventoryItem != null)
                {
                    // remove if equipped
                    RemoveEquippedItem(userInventoryItem.Id);
                    // Not in the list yet → add it
                    // _clanArmoryItems.Add(message.ArmoryItems[0]);
                    userInventoryItem.IsArmoryItem = true;

                    LogDebugError($"userItem: {uItemId} added to clan armory successfully!");
                }
                else
                {
                    LogDebugError($"Add action succeeded on server, but item {uItemId} not found locally.");
                }

                break;

            case ClanArmoryActionType.Remove:
                if (userInventoryItem != null)
                {
                    userInventoryItem.IsArmoryItem = false;
                    LogDebugError($"userItem: {uItemId} removed from clan armory successfully!");
                }

                /*
                    if (clanArmoryItem != null)
                    {
                        // _clanArmoryItems.Remove(clanArmoryItem);
                        if (userInventoryItem != null)
                        {
                            userInventoryItem.IsArmoryItem = false;
                        }

                        LogDebugError($"userItem: {uItemId} removed from clan armory successfully!");
                    }
                */
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
                    {
                        // Add the borrowed item to user inventory
                        _userInventoryItems.Add(clanArmoryItem.UserItem);
                        userInventoryItem = clanArmoryItem.UserItem;
                    }

                    LogDebugError($"userItem: {uItemId} borrowed from clanId: armory successfully!");
                }

                break;

            case ClanArmoryActionType.Return:
                if (clanArmoryItem?.UserItem != null)
                {
                    // remove if equipped
                    RemoveEquippedItem(clanArmoryItem.UserItem.Id);

                    clanArmoryItem.UserItem.IsArmoryItem = true;
                    clanArmoryItem.BorrowedItem = null;
                    if (userInventoryItem != null)
                    {
                        _userInventoryItems.Remove(userInventoryItem);
                        userInventoryItem.IsArmoryItem = true;
                    }

                    LogDebugError($"userItem: {uItemId} returned to clanId: armory successfully!");
                }

                break;

            /*
                        case ClanArmoryActionType.Get:
                            LogDebugError($"Fetched clanId: {message.ClanId} armory list successfully!");
                            if (message.ArmoryItems != null)
                            {
                                SetClanArmoryItems(message.ArmoryItems);
                                foreach (var armoryItem in message.ArmoryItems)
                                {
                                    if (armoryItem.UserItem != null)
                                    {
                                        Debug.Print($"Armory Item Id: {armoryItem.UserItem.ItemId}, Rank: {armoryItem.UserItem.Rank}");
                                    }
                                    else if (armoryItem.BorrowedItem != null)
                                    {
                                        Debug.Print($"Borrowed Item Id: {armoryItem.BorrowedItem.UserItemId} by UserId: {armoryItem.BorrowedItem.BorrowerUserId}");
                                    }
                                }
                            }
                            else
                            {
                                Debug.Print("No items in armory.");
                            }
                            OnClanArmoryUpdated?.Invoke();
                            break;
            */
            default:
                break;
        }
    }
    /*
        private void HandleRecieveArmoryActionResult(ServerSendArmoryActionResult message)
        {
            LogDebug($"[CrpgCharacterLoadoutBehavior] HandleRecieveArmoryActionResult");

            if (!message.Success)
            {
                LogDebugError($"Armory:{message.ActionType} failed: {message.ErrorMessage}");
                return;
            }

            CrpgClanArmoryItem? clanArmoryItem = null;
            CrpgUserItemExtended? userInventoryItem = null;

            // Only look up items if the action is not Get
            if (message.ActionType != ClanArmoryActionType.Get)
            {
                clanArmoryItem = FindClanArmoryItemByUserItemId(message.UserItemId);
                userInventoryItem = FindUserInventoryItemByUserItemId(message.UserItemId);
            }

            switch (message.ActionType)
            {
                case ClanArmoryActionType.Add:
                    if (clanArmoryItem?.UserItem != null)
                    {
                        // Already in the list, just update flags
                        clanArmoryItem.UserItem.IsArmoryItem = true;
                        if (userInventoryItem != null)
                        {
                            userInventoryItem.IsArmoryItem = true;
                        }

                        LogDebugError($"userItem: {message.UserItemId} already exists in clanId: {message.ClanId} armory, updated flags.");
                    }
                    else if (userInventoryItem != null)
                    {
                        // remove if equipped
                        RemoveEquippedItem(userInventoryItem.Id);
                        // Not in the list yet → add it
                        _clanArmoryItems.Add(message.ArmoryItems[0]);
                        userInventoryItem.IsArmoryItem = true;

                        LogDebugError($"userItem: {message.UserItemId} added to clanId: {message.ClanId} armory successfully!");
                    }
                    else
                    {
                        LogDebugError($"Add action succeeded on server, but item {message.UserItemId} not found locally.");
                    }
                    break;

                case ClanArmoryActionType.Remove:
                    if (clanArmoryItem != null)
                    {
                        _clanArmoryItems.Remove(clanArmoryItem);
                        if (userInventoryItem != null)
                        {
                            userInventoryItem.IsArmoryItem = false;
                        }

                        LogDebugError($"userItem: {message.UserItemId} removed from clanId: {message.ClanId} armory successfully!");
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
                        {
                            // Add the borrowed item to user inventory
                            _userInventoryItems.Add(clanArmoryItem.UserItem);
                            userInventoryItem = clanArmoryItem.UserItem;
                        }

                        LogDebugError($"userItem: {message.UserItemId} borrowed from clanId: {message.ClanId} armory successfully!");
                    }

                    break;

                case ClanArmoryActionType.Return:
                    if (clanArmoryItem?.UserItem != null)
                    {
                        // remove if equipped
                        RemoveEquippedItem(clanArmoryItem.UserItem.Id);

                        clanArmoryItem.UserItem.IsArmoryItem = true;
                        clanArmoryItem.BorrowedItem = null;
                        if (userInventoryItem != null)
                        {
                            _userInventoryItems.Remove(userInventoryItem);
                            userInventoryItem.IsArmoryItem = true;
                        }

                        LogDebugError($"userItem: {message.UserItemId} returned to clanId: {message.ClanId} armory successfully!");
                    }

                    break;

                case ClanArmoryActionType.Get:
                    LogDebugError($"Fetched clanId: {message.ClanId} armory list successfully!");
                    if (message.ArmoryItems != null)
                    {
                        SetClanArmoryItems(message.ArmoryItems);
                        foreach (var armoryItem in message.ArmoryItems)
                        {
                            if (armoryItem.UserItem != null)
                            {
                                Debug.Print($"Armory Item Id: {armoryItem.UserItem.ItemId}, Rank: {armoryItem.UserItem.Rank}");
                            }
                            else if (armoryItem.BorrowedItem != null)
                            {
                                Debug.Print($"Borrowed Item Id: {armoryItem.BorrowedItem.UserItemId} by UserId: {armoryItem.BorrowedItem.BorrowerUserId}");
                            }
                        }
                    }
                    else
                    {
                        Debug.Print("No items in armory.");
                    }
                    OnClanArmoryUpdated?.Invoke();
                    break;

                default:
                    message.ErrorMessage = "Unknown action type.";
                    LogDebugError($"HandleUserRequestClanArmoryActionAsync-- Unknown Action: {message.ActionType}");
                    break;
            }

            // Trigger event for GUI only if the action involved a single item
            OnArmoryActionUpdated?.Invoke(message.ActionType, message.UserItemId);
        }
    */
    /// <summary>
    /// Handles a message from the server containing the user's equipped items.
    /// Updates local state and triggers <see cref="OnUserCharacterEquippedItemsUpdated"/>.
    /// </summary>
    /// <param name="message">The server message containing equipped items.</param>
    private void HandleUpdateCrpgCharacterEquippedItems(ServerSendUserCharacterEquippedItems message)
    {
        LogDebug($"HandleUpdateCrpgCharacterEquippedItems");

        if (message.Items == null)
        {
            LogDebugError($"Error in HandleUpdateCrpgCharacterEquippedItems: message.Items was null");
            return;
        }

        SetEquippedItems(message.Items);

        // Trigger event for gui to listen to to know to update
        OnUserCharacterEquippedItemsUpdated?.Invoke();
    }

    /// <summary>
    /// Handles a message from the server containing the user's inventory items.
    /// Updates local state and triggers <see cref="OnUserInventoryUpdated"/>.
    /// </summary>
    /// <param name="message">The server message containing inventory items.</param>
    private void HandleUpdateCrpgUserInventory(ServerSendUserInventoryItems message)
    {
        LogDebug($"HandleUpdateCrpgUserInventory()");

        if (message.Items == null)
        {
            LogDebugError($"Error in HandleUpdateCrpgUserInventory: message.Items was null");
            return;
        }

        SetUserInventoryItems(message.Items);

        LogDebug($"HandleUpdateCrpgUserInventory: items found: {message.Items.Count}");

        // Trigger event for gui to listen to to know to update
        OnUserInventoryUpdated?.Invoke();
    }

    private void HandleUpdateCrpgUserCharacterBasic(ServerSendUserCharacterBasic msg)
    {
        LogDebug("HandleUpdateCrpgUserCharacterBasic");

        if (msg.Character == null)
        {
            LogDebugError("Error in HandleUpdateCrpgUserCharacterBasic: message.Character was null");
            return;
        }

        SetUserCharacterBasic(msg.Character);

        // Trigger event for gui to listen to to know to update
        OnUserCharacterBasicUpdated?.Invoke();
    }

    private void HandleUpdateCharacterStatistics(ServerSendUserCharacterStatistics msg)
    {
        if (msg.CharacterStatistics != null)
        {
            SetUserCharacterStatistics(msg.CharacterStatistics);
        }
    }

    /// <summary>
    /// Handles a message from the server confirming the result of an equip/unequip action.
    /// Updates the affected slot in local equipped items state.
    /// </summary>
    /// <param name="msg">The server message containing the equip result.</param>
    private void HandleEquipItemResult(ServerSendEquipItemResult msg)
    {
        if (!msg.Success)
        {
            LogDebugError($"Equip failed: {msg.ErrorMessage}");
            return;
        }

        var slot = (CrpgItemSlot)msg.SlotIndex;

        CrpgUserItemExtended? userItem = null;

        if (msg.UserItemId != -1)
        {
            // First try to find in user inventory
            userItem = _userInventoryItems.FirstOrDefault(i => i.Id == msg.UserItemId);

            // If not found, maybe it’s a borrowed armory item
            if (userItem == null)
            {
                userItem = _clanArmory?.FindArmoryItem(msg.UserItemId)?.UserItem;
            }

            /*
            // Find inventory entry to populate ItemId
            var inv = _userInventoryItems.FirstOrDefault(i => i.Id == msg.UserItemId);
            if (inv != null)
            {
                userItem = new CrpgUserItemExtended
                {
                    Id = inv.Id,
                    UserId = inv.UserId,
                    Rank = inv.Rank,
                    ItemId = inv.ItemId,
                    IsBroken = inv.IsBroken,
                    CreatedAt = inv.CreatedAt,
                    IsArmoryItem = inv.IsArmoryItem,
                    IsPersonal = inv.IsPersonal,
                };
            }
            */
        }

        // Update only the affected slot
        SetEquippedItem(slot, userItem);
    }

    private void HandleUpdateCharacteristicsResult(ServerSendUpdateCharacteristicsResult message)
    {
        if (!message.Success)
        {
            LogDebugError($"Update Characteristics Failed: {message.ErrorMessage}");
            return;
        }

        if (message.Characteristics == null)
        {
            LogDebugError($"Update Characteristics Failed: Characteristics was null");
            return;
        }

        UserCharacter.Characteristics = message.Characteristics;

        UISoundsHelper.PlayUISound("event:/ui/panels/upgrade");
        // Trigger event for gui to listen to to know to update
        OnUserCharacteristicsUpdated?.Invoke();
    }

    private void HandleConvertCharacteristicsResult(ServerSendConvertCharacteristicsResult message)
    {
        if (!message.Success)
        {
            LogDebugError($"Convert Characteristics Failed: {message.ErrorMessage}");
            return;
        }

        if (message.AttributesPoints < 0 || message.SkillPoints < 0)
        {
            LogDebugError($"Convert Characteristics Failed: AttributePoints or SkillPoints < 0");
            return;
        }

        UserCharacter.Characteristics.Attributes.Points = message.AttributesPoints;
        UserCharacter.Characteristics.Skills.Points = message.SkillPoints;

        OnUserCharacteristicsConverted?.Invoke();
    }

    private readonly bool _debugOn = false;
    private void LogDebug(string message)
    {
        if (_debugOn)
        {
            LogDebug(message, Color.White);
        }
    }

    private void LogDebugError(string message)
    {
        LogDebug(message, Colors.Red);
    }

    private void LogDebug(string message, Color color)
    {
        Debug.Print($"{GetType().Name} {message}");
        InformationManager.DisplayMessage(new InformationMessage($"{GetType().Name} {message}", color));
    }
}

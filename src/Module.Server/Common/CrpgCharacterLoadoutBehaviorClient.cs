using Crpg.Module.Api;
using Crpg.Module.Api.Exceptions;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network;
using Messages.FromClient.ToLobbyServer;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;

internal class CrpgCharacterLoadoutBehaviorClient : MissionNetwork
{
    private readonly List<CrpgEquippedItemExtended> _equippedItems = new();
    private readonly List<CrpgUserItemExtended> _userInventoryItems = new();
    private MissionNetworkComponent? _missionNetworkComponent;

    // Public read-only access
    public IReadOnlyList<CrpgEquippedItemExtended> EquippedItems => _equippedItems;
    public IReadOnlyList<CrpgUserItemExtended> UserInventoryItems => _userInventoryItems;
    public CrpgCharacter UserCharacter { get; private set; } = new();
    public CrpgConstants Constants { get; }

    internal event Action? OnUserInventoryUpdated;
    internal event Action? OnUserCharacterEquippedItemsUpdated;
    internal event Action? OnUserCharacterBasicUpdated;
    internal event Action<CrpgItemSlot>? OnSlotUpdated;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _missionNetworkComponent = Mission.GetMissionBehavior<MissionNetworkComponent>();
        if (_missionNetworkComponent != null)
        {
            _missionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
        }
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        if (_missionNetworkComponent != null)
        {
            _missionNetworkComponent.OnMyClientSynchronized -= OnMyClientSynchronized;
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
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestGetInventoryItems());
        GameNetwork.EndModuleEventAsClient();

        // Equipped items
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestGetEquippedItems());
        GameNetwork.EndModuleEventAsClient();

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
    /// Updates the list and triggers the <see cref="OnSlotUpdated"/> event.
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

        OnSlotUpdated?.Invoke(slot); // notify VM of only this slot
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

    internal void SetUserCharacterBasic(CrpgCharacter crpgCharacter)
    {
        UserCharacter = crpgCharacter;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<ServerSendUserInventoryItems>(HandleUpdateCrpgUserInventory); // recieve user items from server
        registerer.Register<ServerSendUserCharacterEquippedItems>(HandleUpdateCrpgCharacterEquippedItems); // recieve equipped items for character from server
        registerer.Register<ServerSendEquipItemResult>(HandleEquipItemResult); // recieve result of attempt to equip item in slot on api from server
        registerer.Register<ServerSendUserCharacterBasic>(HandleUpdateCrpgUserCharacterBasic); // recieve character basic from server
    }

    private void OnMyClientSynchronized()
    {
        RequestGetUpdatedEquipmentAndItems();
    }

    /// <summary>
    /// Handles a message from the server containing the user's equipped items.
    /// Updates local state and triggers <see cref="OnUserCharacterEquippedItemsUpdated"/>.
    /// </summary>
    /// <param name="message">The server message containing equipped items.</param>
    private void HandleUpdateCrpgCharacterEquippedItems(ServerSendUserCharacterEquippedItems message)
    {
        string debugString = $"HandleUpdateCrpgCharacterEquippedItems";
        InformationManager.DisplayMessage(new InformationMessage(debugString));
        Debug.Print(debugString);

        if (message.Items == null)
        {
            debugString = $"Error in HandleUpdateCrpgCharacterEquippedItems: message.Items was null";
            InformationManager.DisplayMessage(new InformationMessage(debugString));
            Debug.Print(debugString);
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
        string debugString = $"HandleUpdateCrpgUserInventory";
        InformationManager.DisplayMessage(new InformationMessage(debugString));
        Debug.Print(debugString);

        if (message.Items == null)
        {
            debugString = $"Error in HandleUpdateCrpgUserInventory: message.Items was null";
            InformationManager.DisplayMessage(new InformationMessage(debugString));
            Debug.Print(debugString);
            return;
        }

        SetUserInventoryItems(message.Items);

        debugString = $"HandleUpdateCrpgUserInventory: items found: {message.Items.Count}";
        InformationManager.DisplayMessage(new InformationMessage(debugString));
        Debug.Print(debugString);

        // Trigger event for gui to listen to to know to update
        OnUserInventoryUpdated?.Invoke();
    }

    private void HandleUpdateCrpgUserCharacterBasic(ServerSendUserCharacterBasic msg)
    {
        string debugString = $"HandleUpdateCrpgUserCharacterBasic";
        InformationManager.DisplayMessage(new InformationMessage(debugString));
        Debug.Print(debugString);

        if (msg.Character == null)
        {
            debugString = $"Error in HandleUpdateCrpgUserCharacterBasic: message.Character was null";
            InformationManager.DisplayMessage(new InformationMessage(debugString));
            Debug.Print(debugString);
            return;
        }

        SetUserCharacterBasic(msg.Character);

        // Trigger event for gui to listen to to know to update
        OnUserCharacterBasicUpdated?.Invoke();
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
            InformationManager.DisplayMessage(new InformationMessage($"Equip failed: {msg.ErrorMessage}"));
            return;
        }

        var slot = (CrpgItemSlot)msg.SlotIndex;

        CrpgUserItemExtended? userItem = null;

        if (msg.UserItemId != -1)
        {
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
        }

        // Update only the affected slot
        SetEquippedItem(slot, userItem);
    }
}

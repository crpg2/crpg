using Crpg.Module.Api;
using Crpg.Module.Api.Exceptions;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;

internal class CrpgInventoryBehaviorClient : MissionNetwork
{
    // Backing fields
    private readonly List<CrpgEquippedItemExtended> _equippedItems = new();
    private readonly List<CrpgUserItemExtended> _userInventoryItems = new();

    // Public read-only access
    public IReadOnlyList<CrpgEquippedItemExtended> EquippedItems => _equippedItems;
    public IReadOnlyList<CrpgUserItemExtended> UserInventoryItems => _userInventoryItems;

    internal static event Action? OnUserInventoryUpdated;
    internal static event Action? OnUserEquippedItemsUpdated;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        // Initialize or fetch inventory/equipped items here
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestGetInventoryItems());
        GameNetwork.EndModuleEventAsClient();

        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestGetEquippedItems());
        GameNetwork.EndModuleEventAsClient();
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
    }

    /// <summary>
    /// Builds a TaleWorlds Equipment object from the current equipped items.
    /// </summary>
    public Equipment GetEquipment()
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
                    // You can add more properties if needed
                },
            })
            .ToList();

        // Build Equipment using CrpgCharacterBuilder
        return CrpgCharacterBuilder.CreateCharacterEquipment(baseEquippedItems);
    }

    // Internal methods to modify the lists
    internal void SetEquippedItems(IEnumerable<CrpgEquippedItemExtended> items)
    {
        _equippedItems.Clear();
        _equippedItems.AddRange(items);
    }

    internal void SetUserInventoryItems(IEnumerable<CrpgUserItemExtended> items)
    {
        _userInventoryItems.Clear();
        _userInventoryItems.AddRange(items);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        // registerer.Register<UpdateCrpgUserItems>(HandleUpdateCrpgUserInventory); // recieve user items from server
        // need to add reciever user character equipped items from server
    }

    private void HandleUpdateCrpgUserInventory(UpdateCrpgUserItems message)
    {
        string debugString = $"HandleUpdateCrpgUserInventory";
        InformationManager.DisplayMessage(new InformationMessage(debugString));
        Debug.Print(debugString);

        if (message.Peer == null || message.Items == null)
        {
            debugString = $"Error in HandleUpdateCrpgUserInventory: message.Peer or message.Items was null";
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

}
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

namespace Crpg.Module.Common.Inventory;

internal class CrpgInventoryBehaviorClient : MissionNetwork
{
    // Backing fields
    private readonly List<CrpgEquippedItemExtended> _equippedItems = new();
    private readonly List<CrpgUserItemExtended> _userInventoryItems = new();

    // Public read-only access
    public IReadOnlyList<CrpgEquippedItemExtended> EquippedItems => _equippedItems;
    public IReadOnlyList<CrpgUserItemExtended> UserInventoryItems => _userInventoryItems;

    internal static event Action? OnUserInventoryUpdated;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        // Initialize or fetch inventory/equipped items here
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
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
        registerer.Register<UpdateCrpgUserItems>(HandleUpdateCrpgUserInventory); // recieve user items from server
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
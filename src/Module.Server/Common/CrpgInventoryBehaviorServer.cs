using System.Globalization;
using System.Text;
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

internal class CrpgInventoryBehaviorServer : MissionNetwork
{
    private readonly ICrpgClient _crpgClient;

    public CrpgInventoryBehaviorServer(ICrpgClient crpgClient)
    {
        _crpgClient = crpgClient;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<UserRequestGetInventoryItems>(HandleUserRequestGetInventoryItems);
        registerer.Register<UserRequestGetEquippedItems>(HandleUserRequestGetEquippedItems);
        registerer.Register<UserRequestEquipCharacterItem>(HandleUserRequestEquipCharacterItem);
    }

    private bool HandleUserRequestGetInventoryItems(NetworkCommunicator networkPeer, UserRequestGetInventoryItems message)
    {
        Debug.Print("HandleUserRequestGetInventoryItems()");
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        if (crpgPeer?.User != null)
        {
            _ = UpdateUserItemsAsync(networkPeer);

            return true;
        }

        return false;
    }

    private bool HandleUserRequestEquipCharacterItem(NetworkCommunicator networkPeer, UserRequestEquipCharacterItem message)
    {
        var slot = message.Slot;
        int? userItemId = message.UserItemId;

        Debug.Print($"OnRequestEquipCharacterItem() slot: {slot} uItemId: {userItemId}");

        // remove item
        if (userItemId == -1)
        {
            userItemId = null;
        }

        // Build API request directly
        var apiRequest = new CrpgGameCharacterItemsUpdateRequest
        {
            Items = new List<CrpgEquippedItemId>
            {
                new CrpgEquippedItemId
                {
                    Slot = slot,
                    UserItemId = userItemId,
                },
            },
        };

        // Debug print for verification
        Debug.Print("API Request Items:");
        foreach (var eq in apiRequest.Items)
        {
            if (eq.UserItemId != null)
            {
                Debug.Print($"(equip item) Slot: {eq.Slot} UserItem.Id: {eq.UserItemId}");
            }
            else
            {
                Debug.Print($"(unequip item) Slot: {eq.Slot} UserItem: null");
            }
        }

        try
        {
            _ = TryEquipCharacterItemsAsync(networkPeer, apiRequest);
        }
        catch (Exception ex)
        {
            Debug.Print($"Failed to equip/unequip user items for {networkPeer.UserName}: {ex}");
        }

        return true;
    }

    private bool HandleUserRequestGetEquippedItems(NetworkCommunicator networkPeer, UserRequestGetEquippedItems message)
    {
        _ = GetUserEquippedItemsAsync(networkPeer);
        return true;
    }

    private async Task TryEquipCharacterItemsAsync(NetworkCommunicator networkPeer, CrpgGameCharacterItemsUpdateRequest apiRequest)
    {
        try
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            var crpgUser = crpgPeer?.User;
            var crpgCharacter = crpgUser?.Character;

            if (crpgUser == null || crpgCharacter == null)
            {
                Debug.Print($"[TryEquipCharacterItemsAsync] No user data found for peer {networkPeer.UserName}");
                return;
            }

            var apiRes = await _crpgClient.UpdateCharacterEquippedItemsAsync(crpgUser.Id, crpgCharacter.Id, apiRequest);

            if (apiRes == null)
            {
                Debug.Print($"[TryEquipCharacterItemsAsync] apiRes was null");
                return;
            }

            if (apiRes.Errors == null || apiRes.Errors.Count == 0)
            {
                var updatedItems = apiRes.Data;
                if (updatedItems == null)
                {
                    Debug.Print($"[TryEquipCharacterItemsAsync] apiRes.Data was null");
                    return;
                }

                foreach (var equippedItem in updatedItems)
                {
                    if (equippedItem.UserItemId != null)
                    {
                        Debug.Print($"Slot: {equippedItem.Slot}, UserItemId: {equippedItem.UserItemId}");
                    }

                    // TODO: update client here
                }
            }
            else
            {
                var firstError = apiRes.Errors[0];
                Debug.Print($"Error Code: {firstError.Code}, Message: {firstError.Type}, Detail: {firstError.Detail}");
            }
        }
        catch (Exception ex)
        {
            Debug.Print($"[TryEquipCharacterItemsAsync] Exception for peer {networkPeer.UserName}: {ex}");
            // optionally log to a server log for debugging
        }
    }

    private async Task GetUserEquippedItemsAsync(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();

        if (crpgPeer?.User == null || crpgPeer?.User?.Character == null)
        {
            Debug.Print($"[GetUserEquippedItemsAsync] No user data or character found for peer {networkPeer.UserName}");
            return;
        }

        CrpgUser crpgUser = crpgPeer.User;

        try
        {
            Debug.Print($"[GetUserEquippedItemsAsync] Sending Api Request");
            var equippedItemsRes = await _crpgClient.GetCharacterEquippedItemsAsync(crpgUser.Id, crpgUser.Character.Id);
            if (equippedItemsRes.Errors != null)
            {
                Debug.Print($"Errors in response");
            }

            if (equippedItemsRes.Data != null)
            {
                var equippedItems = equippedItemsRes.Data;
                Debug.Print($"User {crpgUser.Id} Character {crpgUser.Character.Name} Equipped Items.");
                foreach (var item in equippedItems)
                {
                    Debug.Print($"slot {item.Slot} itemId: {item.UserItem.ItemId} Id:{item.UserItem.Id}");
                }

                SynchronizeUserCharacterEquippedItemsToPeer(networkPeer, equippedItems);
            }
            else
            {
                Debug.Print($"equippedItemsRes.Data was null");
            }
        }
        catch (Exception e)
        {
            Debug.Print($"Error fetching equipped items for peer {networkPeer.UserName} character {crpgUser.Character.Name}: {e}");
        }
    }

    private async Task UpdateUserItemsAsync(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        VirtualPlayer vp = networkPeer.VirtualPlayer;
        string userName = vp.UserName;

        if (crpgPeer?.User == null)
        {
            Debug.Print($"[UpdateUserItemsAsync] No user data found for peer {networkPeer.UserName}");
            return;
        }

        try
        {
            CrpgUser crpgUser = crpgPeer.User;
            var itemsRes = await _crpgClient.GetUserItemsAsync(crpgUser.Id);
            if (itemsRes.Data != null)
            {
                var userItems = itemsRes.Data;
                // crpgPeer.User.Items = userItems;

                Debug.Print($"User {crpgUser.Id} has {userItems.Count} items.");
                foreach (var item in userItems)
                {
                    Debug.Print($"--- Item ---");

                    // Print CrpgUserItemExtended properties
                    foreach (var prop in typeof(CrpgUserItemExtended).GetProperties())
                    {
                        var value = prop.GetValue(item);
                        Debug.Print($"{prop.Name}: {value}");
                    }
                }

                SynchronizeUserInventoryItemsToPeer(networkPeer, userItems);
            }

            else
            {
                Debug.Print($"Failed to fetch items for user {crpgUser.Id}");
            }
        }
        catch (Exception e)
        {
            Debug.Print($"Error fetching items for peer {networkPeer.UserName}: {e}");
        }
    }

    private void SynchronizeUserInventoryItemsToPeer(NetworkCommunicator networkPeer, IList<CrpgUserItemExtended> items)
    {
        try
        {
            Debug.Print("SynchronizeUserInventoryItemsToPeer()");

            if (networkPeer == null || items == null)
            {
                Debug.Print("networkPeer or items is null");
                return;
            }

            Debug.Print($"Sending {items.Count} items to peer {networkPeer.Index}");

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerSendUserInventoryItems { Items = items });
            GameNetwork.EndModuleEventAsServer();
        }
        catch (Exception ex)
        {
            Debug.Print($"SynchronizeUserInventoryItemsToPeer() exception: {ex}");
            Console.WriteLine($"SynchronizeUserInventoryItemsToPeer() exception: {ex}");
        }
    }

    private void SynchronizeUserCharacterEquippedItemsToPeer(NetworkCommunicator networkPeer, IList<CrpgEquippedItemExtended> items)
    {
        try
        {
            Debug.Print("SynchronizeUserCharacterEquippedItemsToPeer()");

            if (networkPeer == null || items == null)
            {
                Debug.Print("networkPeer or items is null");
                return;
            }

            Debug.Print($"Sending {items.Count} items to peer {networkPeer.Index}");

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerSendUserCharacterEquippedItems { Items = items });
            GameNetwork.EndModuleEventAsServer();
        }
        catch (Exception ex)
        {
            Debug.Print($"SynchronizeUserCharacterEquippedItemsToPeer() exception: {ex}");
            Console.WriteLine($"SynchronizeUserCharacterEquippedItemsToPeer() exception: {ex}");
        }
    }
}

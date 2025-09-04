using System.Globalization;
using System.Text;
using Crpg.Module.Api;
using Crpg.Module.Api.Exceptions;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
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

internal class CrpgCharacterLoadoutBehaviorServer : MissionNetwork
{
    private readonly ICrpgClient _crpgClient;

    public CrpgCharacterLoadoutBehaviorServer(ICrpgClient crpgClient)
    {
        _crpgClient = crpgClient;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<UserRequestGetInventoryItems>(HandleUserRequestGetInventoryItems);
        registerer.Register<UserRequestGetEquippedItems>(HandleUserRequestGetEquippedItems);
        registerer.Register<UserRequestEquipCharacterItem>(HandleUserRequestEquipCharacterItem);
        registerer.Register<UserRequestGetCharacterBasic>(HandleUserRequestGetCharacterBasic);
        registerer.Register<UserRequestUpdateCharacterCharacteristics>(HandleUserRequestUpdateCharacterCharacteristics);
    }

    /// <summary>
    /// Handles a client's request to get all items in their inventory.
    /// Initiates an asynchronous update of the user's inventory items.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <param name="message">The message containing the request (empty payload).</param>
    /// <returns>Returns true if the request was handled, false otherwise.</returns>
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

    /// <summary>
    /// Handles a client's request to fetch basic information about their character
    /// and initiates an asynchronous request to retrieve the data from the API.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <param name="message">The message containing the request (empty payload).</param>
    /// <returns>Returns true if the request was handled successfully.</returns>
    private bool HandleUserRequestGetCharacterBasic(NetworkCommunicator networkPeer, UserRequestGetCharacterBasic message)
    {
        Debug.Print("HandleUserRequestGetCharacterBasic()");
        _ = GetUserCharacterBasicAsync(networkPeer);
        return true;
    }

    private bool HandleUserRequestUpdateCharacterCharacteristics(NetworkCommunicator networkPeer, UserRequestUpdateCharacterCharacteristics message)
    {
        Debug.Print("HandleUserRequestUpdateCharacterCharacteristics");

        var apiRequest = new CrpgGameCharacterCharacteristicsUpdateRequest
        {
            Characteristics = message.Characteristics,
        };

        _ = TryUpdateCharacterCharacteristicsAsync(networkPeer, apiRequest);
        return true;
    }

    private async Task TryUpdateCharacterCharacteristicsAsync(NetworkCommunicator networkPeer, CrpgGameCharacterCharacteristicsUpdateRequest apiRequest)
    {
        try
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            var crpgUser = crpgPeer?.User;
            var crpgCharacter = crpgUser?.Character;

            if (crpgUser == null || crpgCharacter == null)
            {
                Debug.Print($"[TryUpdateCharacterCharacteristicsAsync] No user/character data found for peer {networkPeer.UserName}");
                return;
            }

            if (apiRequest?.Characteristics == null)
            {
                Debug.Print("[TryUpdateCharacterCharacteristicsAsync] No characteristics provided in the request");
                return;
            }

            // Send the request to the API
            var apiRes = await _crpgClient.UpdateCharacterCharacteristicsAsync(crpgUser.Id, crpgCharacter.Id, apiRequest);

            // Send respones to the Client
            SynchronizeUserTryUpdateCharacteristicsResult(networkPeer, apiRequest, apiRes);
        }
        catch (Exception ex)
        {
            Debug.Print($"[TryUpdateCharacterCharacteristicsAsync] Exception for peer {networkPeer.UserName}: {ex}");
        }
    }

    private async Task GetUserCharacterBasicAsync(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();

        if (crpgPeer?.User == null || crpgPeer?.User?.Character == null)
        {
            Debug.Print($"[GetUserCharacterBasicAsync] No user data or character found for peer {networkPeer.UserName}");
            return;
        }

        CrpgUser crpgUser = crpgPeer.User;

        try
        {
            Debug.Print($"[GetUserCharacterBasicAsync] Sending Api Request");
            var characterRes = await _crpgClient.GetUserCharacterBasicAsync(crpgUser.Id, crpgUser.Character.Id);
            if (characterRes.Errors != null)
            {
                Debug.Print($"Errors in response");
            }

            if (characterRes.Data != null)
            {
                var crpgCharacterBasic = characterRes.Data;
                Debug.Print($"User {crpgUser.Id} Character {crpgUser.Character.Name} Basic fetched successfully");

                SynchronizeUserCharacterBasicToPeer(networkPeer, crpgCharacterBasic);
            }
            else
            {
                Debug.Print($"characterRes.Data was null");
            }
        }
        catch (Exception e)
        {
            Debug.Print($"Error fetching character basic for peer {networkPeer.UserName} character {crpgUser.Character.Name}: {e}");
        }
    }

    /// <summary>
    /// Handles a client's request to equip or unequip an item for their character.
    /// Builds the API request and forwards it to TryEquipCharacterItemsAsync.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <param name="message">The message containing slot and item information.</param>
    /// <returns>Returns true if the request was handled, false otherwise.</returns>
    private bool HandleUserRequestEquipCharacterItem(NetworkCommunicator networkPeer, UserRequestEquipCharacterItem message)
    {
        var slot = message.Slot;
        int? userItemId = message.UserItemId;

        Debug.Print($"HandleUserRequestEquipCharacterItem() slot: {slot} uItemId: {userItemId}");

        // unequip item if -1. send null to Request
        if (userItemId == -1)
        {
            userItemId = null;
        }

        // Build API request directly
        var apiRequest = new CrpgGameCharacterItemsUpdateRequest
        {
            Items = new List<CrpgEquippedItemId>
            {
                new() // CrpgEquippedItemId
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

    /// <summary>
    /// Handles a client's request to get their currently equipped items.
    /// Initiates an asynchronous fetch of equipped items and sends them to the client.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <param name="message">The message containing the request (empty payload).</param>
    /// <returns>Returns true indicating the request was handled.</returns>
    private bool HandleUserRequestGetEquippedItems(NetworkCommunicator networkPeer, UserRequestGetEquippedItems message)
    {
        _ = GetUserEquippedItemsAsync(networkPeer);
        return true;
    }

    /// <summary>
    /// Sends a request to the API to update character equipped items and
    /// synchronizes the result back to the client.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <param name="apiRequest">The requested changes to character equipment.</param>
    private async Task TryEquipCharacterItemsAsync(NetworkCommunicator networkPeer, CrpgGameCharacterItemsUpdateRequest apiRequest)
    {
        try
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            var crpgUser = crpgPeer?.User;
            var crpgCharacter = crpgUser?.Character;

            if (crpgUser == null || crpgCharacter == null)
            {
                Debug.Print($"[TryEquipCharacterItemsAsync] No user/character data found for peer {networkPeer.UserName}");
                return;
            }

            if (apiRequest?.Items == null || apiRequest.Items.Count == 0)
            {
                Debug.Print("[TryEquipCharacterItemsAsync] No items provided in the request");
                return;
            }

            // Send the request to the API
            var apiRes = await _crpgClient.UpdateCharacterEquippedItemsAsync(crpgUser.Id, crpgCharacter.Id, apiRequest);

            // Send respones to the Client
            SynchronizeUserTryEquipItemResult(networkPeer, apiRequest, apiRes);
        }
        catch (Exception ex)
        {
            Debug.Print($"[TryEquipCharacterItemsAsync] Exception for peer {networkPeer.UserName}: {ex}");
        }
    }

    /// <summary>
    /// Fetches the user's character's currently equipped items from the API
    /// and synchronizes them with the client.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Fetches all inventory items for the given user from the API
    /// and sends them to the client.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
                        object? value = prop.GetValue(item);
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

    /// <summary>
    /// Sends a list of inventory items to a specific client.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <param name="items">The list of inventory items to send.</param>
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

    /// <summary>
    /// Sends a list of currently equipped items to a specific client.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <param name="items">The list of equipped items to send.</param>
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

    private void SynchronizeUserCharacterBasicToPeer(NetworkCommunicator networkPeer, CrpgCharacter crpgCharacter)
    {
        try
        {
            Debug.Print("SynchronizeUserCharacterEquippedItemsToPeer()");

            if (networkPeer == null || crpgCharacter == null)
            {
                Debug.Print("networkPeer or crpgCharacter is null");
                return;
            }

            Debug.Print($"Sending Character basic ({crpgCharacter.Name}) to peer {networkPeer.Index}");

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerSendUserCharacterBasic { Character = crpgCharacter });
            GameNetwork.EndModuleEventAsServer();
        }
        catch (Exception ex)
        {
            Debug.Print($"SynchronizeUserCharacterEquippedItemsToPeer() exception: {ex}");
            Console.WriteLine($"SynchronizeUserCharacterEquippedItemsToPeer() exception: {ex}");
        }
    }

    private void SynchronizeUserTryUpdateCharacteristicsResult(NetworkCommunicator networkPeer, CrpgGameCharacterCharacteristicsUpdateRequest apiRequest,
    private void SynchronizeUserTryUpdateCharacteristicsResult(
        NetworkCommunicator networkPeer,
        CrpgGameCharacterCharacteristicsUpdateRequest apiRequest,
        CrpgResult<CrpgCharacterCharacteristics>? apiRes) // <-- mark nullable
    {
        if (apiRes == null)
        {
            return;
        }

        string newErrorMessage = string.Empty;
        bool wasSuccess = true;

        // Safe check for errors
        if (apiRes.Errors == null || apiRes.Errors.Count == 0)
        {
            Debug.Print("Characteristics received on server");

            if (apiRes.Data == null)
            {
                Debug.Print("Characteristics Data was null");
                apiRes.Data = new CrpgCharacterCharacteristics(); // create safe default
                wasSuccess = false;
                newErrorMessage = "apiRes Data was null";
            }

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerSendUpdateCharacteristicsResult
            {
                Success = wasSuccess,
                Characteristics = apiRes.Data,
                ErrorMessage = newErrorMessage,
            });
            GameNetwork.EndModuleEventAsServer();
            Debug.Print("Characteristics sent to client");
        }
        else
        {
            // iterate safely
            if (apiRes.Errors != null)
            {
                foreach (var error in apiRes.Errors)
                {
                    Debug.Print($"Characteristics failed to update. Error: {error?.Detail}");
                }

                var firstError = apiRes.Errors.FirstOrDefault();
                string errorMessage = firstError?.Detail ?? "Characteristics failed to update";

                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerSendUpdateCharacteristicsResult
                {
                    Success = false,
                    Characteristics = new CrpgCharacterCharacteristics(),
                    ErrorMessage = errorMessage,
                });
                GameNetwork.EndModuleEventAsServer();
            }
        }
    }

    /// <summary>
    /// Sends the results of an equip/unequip request back to the client.
    /// Only sends updates for items that actually changed; if there are errors,
    /// sends a failure message for the first requested slot.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <param name="apiRequest">The request containing the items attempted to equip/unequip.</param>
    /// <param name="apiRes">The API response containing updated items and/or errors.</param>
    private void SynchronizeUserTryEquipItemResult(NetworkCommunicator networkPeer, CrpgGameCharacterItemsUpdateRequest apiRequest,
        CrpgResult<IList<CrpgEquippedItemId>> apiRes)
    {
        if (apiRes == null)
        {
            return;
        }

        if (apiRes.Errors == null || apiRes.Errors.Count == 0)
        {
            // Iterate requested items and send updates only for items that actually changed
            foreach (var requestedItem in apiRequest.Items)
            {
                var updatedItem = apiRes.Data?.FirstOrDefault(up => up.Slot == requestedItem.Slot);

                int userItemId = updatedItem?.UserItemId ?? -1; // -1 indicates unequipped

                Debug.Print($"Slot {requestedItem.Slot} updated. UserItemId = {userItemId}");

                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerSendEquipItemResult
                {
                    Success = true,
                    SlotIndex = (int)requestedItem.Slot,
                    UserItemId = userItemId,
                });
                GameNetwork.EndModuleEventAsServer();
            }
        }
        else
        {
            // Send failure for the first requested slot
            var failedSlot = apiRequest.Items[0].Slot;

            foreach (var error in apiRes.Errors)
            {
                Debug.Print($"Slot {failedSlot} failed to update. Error: {error.Detail}");
            }

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerSendEquipItemResult
            {
                Success = false,
                SlotIndex = (int)failedSlot,
                UserItemId = -1,
                ErrorMessage = apiRes.Errors[0].Detail ?? "Failed to equip/unequip item",
            });
            GameNetwork.EndModuleEventAsServer();
        }
    }
}

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
        registerer.Register<UserRequestConvertCharacteristics>(HandleUserRequestConvertCharacteristics);
        registerer.Register<UserRequestClanArmoryAction>(HandleUserRequestClanArmoryAction);
    }

    private bool HandleUserRequestClanArmoryAction(NetworkCommunicator networkPeer, UserRequestClanArmoryAction message)
    {
        if (message == null)
            return false;

        // Fire-and-forget async call
        _ = HandleUserRequestClanArmoryActionAsync(networkPeer, message);

        return true;
    }

    private async Task HandleUserRequestClanArmoryActionAsync(NetworkCommunicator networkPeer, UserRequestClanArmoryAction message)
    {
        var request = message.Request;

        var result = new ServerSendArmoryActionResult
        {
            ActionType = request.ActionType,
            ClanId = request.ClanId,
            UserItemId = request.UserItemId,
            UserId = request.UserId,
            Success = false,
            ErrorMessage = string.Empty,
            ArmoryItems = new List<CrpgClanArmoryItem>(),
        };

        try
        {
            switch (request.ActionType)
            {
                case ClanArmoryActionType.Add:
                    {
                        var apiRes = await _crpgClient.ClanArmoryAddItemAsync(request.ClanId,
                            new CrpgGameClanArmoryAddItemRequest { UserItemId = request.UserItemId, UserId = request.UserId });

                        if (apiRes?.Errors?.Count > 0)
                        {
                            result.ErrorMessage = string.Join("; ", apiRes.Errors.Select(e => e.Detail));
                            Debug.Print($"Add Failed: {result.ErrorMessage}");
                        }
                        else
                        {
                            result.Success = true;
                            Debug.Print($"Added item {request.UserItemId} to clan {request.ClanId}");
                        }
                    }

                    break;

                case ClanArmoryActionType.Remove:
                    {
                        var apiRes = await _crpgClient.RemoveClanArmoryItemAsync(request.ClanId, request.UserItemId, request.UserId);

                        if (apiRes?.Errors?.Count > 0)
                        {
                            result.ErrorMessage = string.Join("; ", apiRes.Errors.Select(e => e.Detail));
                            Debug.Print($"Remove Failed: {result.ErrorMessage}");
                        }
                        else
                        {
                            result.Success = true;
                            Debug.Print($"Removed item {request.UserItemId} from clan {request.ClanId}");
                        }
                    }

                    break;

                case ClanArmoryActionType.Borrow:
                    {
                        var apiRes = await _crpgClient.ClanArmoryBorrowItemAsync(request.ClanId, request.UserItemId,
                            new CrpgGameBorrowClanArmoryItemRequest { UserId = request.UserId });

                        if (apiRes?.Errors?.Count > 0)
                        {
                            result.ErrorMessage = string.Join("; ", apiRes.Errors.Select(e => e.Detail));
                            Debug.Print($"Borrow Failed: {result.ErrorMessage}");
                        }
                        else
                        {
                            result.Success = true;
                            Debug.Print($"Borrowed item {request.UserItemId} from clan {request.ClanId}");
                        }
                    }

                    break;

                case ClanArmoryActionType.Return:
                    {
                        var apiRes = await _crpgClient.ClanArmoryReturnItemAsync(request.ClanId, request.UserItemId,
                            new CrpgGameBorrowClanArmoryItemRequest { UserId = request.UserId });

                        if (apiRes?.Errors?.Count > 0)
                        {
                            result.ErrorMessage = string.Join("; ", apiRes.Errors.Select(e => e.Detail));
                            Debug.Print($"Return Failed: {result.ErrorMessage}");
                        }
                        else
                        {
                            result.Success = true;
                            Debug.Print($"Returned item {request.UserItemId} to clan {request.ClanId}");
                        }
                    }

                    break;

                case ClanArmoryActionType.Get:
                    {
                        var apiRes = await _crpgClient.GetClanArmoryAsync(request.ClanId, request.UserId);

                        if (apiRes?.Errors?.Count > 0)
                        {
                            result.ErrorMessage = string.Join("; ", apiRes.Errors.Select(e => e.Detail));
                            Debug.Print($"Get Failed: {result.ErrorMessage}");
                        }
                        else if (apiRes?.Data != null)
                        {
                            result.Success = true;
                            result.ArmoryItems = apiRes.Data.ToList();

                            Debug.Print($"Fetch armory items for clan {request.ClanId}, count: {result.ArmoryItems.Count}");
                            foreach (var item in result.ArmoryItems)
                            {
                                Debug.Print($"- userItemId: {item.UserItem?.Id}, borrowerUserId: {item?.BorrowedItem?.BorrowerUserId}, rank: {item?.UserItem?.Rank}");
                            }
                        }
                        else
                        {
                            Debug.Print("Armory is empty.");
                        }
                    }

                    break;

                default:
                    result.ErrorMessage = "Unknown action type.";
                    Debug.Print($"Unknown Action: {request.ActionType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            Debug.Print($"HandleUserRequestClanArmoryActionAsync exception for peer {networkPeer.UserName}: {ex}");
        }

        // Send the result to the client
        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(result);
        GameNetwork.EndModuleEventAsServer();
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

    /// <summary>
    /// Handles a client's request to update their characters characteristics
    /// and initiates an asynchronous request to the API.
    /// </summary>
    /// <param name="networkPeer">The network peer representing the connected client.</param>
    /// <param name="message">The message containing the request (new Characteristics).</param>
    /// <returns>Returns true if the request was handled successfully.</returns>
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

    private bool HandleUserRequestConvertCharacteristics(NetworkCommunicator networkPeer, UserRequestConvertCharacteristics message)
    {
        Debug.Print("HandleUserRequestConvertCharacteristics");

        var apiRequest = message.ConversionRequest;
        _ = TryConvertCharacterCharacteristicsAsync(networkPeer, apiRequest);
        return true;
    }

    private async Task TryConvertCharacterCharacteristicsAsync(NetworkCommunicator networkPeer, CrpgGameCharacteristicConversionRequest apiRequest)
    {
        try
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            var crpgUser = crpgPeer?.User;
            var crpgCharacter = crpgUser?.Character;

            if (crpgUser == null || crpgCharacter == null)
            {
                Debug.Print($"[TryConvertCharacterCharacteristicsAsync] No user/character data found for peer {networkPeer.UserName}", 0, Debug.DebugColor.Red);
                return;
            }

            if (apiRequest.Equals == null)
            {
                Debug.Print("[TryConvertCharacterCharacteristicsAsync] No Conversion provided in the request", 0, Debug.DebugColor.Red);
                return;
            }

            // Send the request to the API
            var apiRes = await _crpgClient.ConvertCharacterCharacteristicsAsync(crpgUser.Id, crpgCharacter.Id, apiRequest);

            // Send respones to the Client
            SynchronizeUserTryConverCharacteristicsResult(networkPeer, apiRequest, apiRes);
        }
        catch (Exception ex)
        {
            Debug.Print($"[TryConvertCharacterCharacteristicsAsync] Exception for peer {networkPeer.UserName}: {ex}", 0, Debug.DebugColor.Red);
        }
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
                Debug.Print($"[TryUpdateCharacterCharacteristicsAsync] No user/character data found for peer {networkPeer.UserName}", 0, Debug.DebugColor.Red);
                return;
            }

            if (apiRequest?.Characteristics == null)
            {
                Debug.Print("[TryUpdateCharacterCharacteristicsAsync] No characteristics provided in the request", 0, Debug.DebugColor.Red);
                return;
            }

            // Send the request to the API
            var apiRes = await _crpgClient.UpdateCharacterCharacteristicsAsync(crpgUser.Id, crpgCharacter.Id, apiRequest);

            // Send respones to the Client
            SynchronizeUserTryUpdateCharacteristicsResult(networkPeer, apiRequest, apiRes);
        }
        catch (Exception ex)
        {
            Debug.Print($"[TryUpdateCharacterCharacteristicsAsync] Exception for peer {networkPeer.UserName}: {ex}", 0, Debug.DebugColor.Red);
        }
    }

    private async Task GetUserCharacterBasicAsync(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();

        if (crpgPeer?.User == null || crpgPeer?.User?.Character == null)
        {
            Debug.Print($"[GetUserCharacterBasicAsync] No user data or character found for peer {networkPeer.UserName}", 0, Debug.DebugColor.Red);
            return;
        }

        CrpgUser crpgUser = crpgPeer.User;

        try
        {
            Debug.Print($"[GetUserCharacterBasicAsync] Sending Api Request");
            var characterRes = await _crpgClient.GetUserCharacterBasicAsync(crpgUser.Id, crpgUser.Character.Id);
            if (characterRes.Errors != null)
            {
                Debug.Print($"Errors in response", 0, Debug.DebugColor.Red);
            }

            if (characterRes.Data != null)
            {
                var crpgCharacterBasic = characterRes.Data;
                Debug.Print($"User {crpgUser.Id} Character {crpgUser.Character.Name} Basic fetched successfully");

                SynchronizeUserCharacterBasicToPeer(networkPeer, crpgCharacterBasic);
            }
            else
            {
                Debug.Print($"characterRes.Data was null", 0, Debug.DebugColor.Red);
            }
        }
        catch (Exception e)
        {
            Debug.Print($"Error fetching character basic for peer {networkPeer.UserName} character {crpgUser.Character.Name}: {e}", 0, Debug.DebugColor.Red);
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
            Debug.Print($"Failed to equip/unequip user items for {networkPeer.UserName}: {ex}", 0, Debug.DebugColor.Red);
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
                Debug.Print($"[TryEquipCharacterItemsAsync] No user/character data found for peer {networkPeer.UserName}", 0, Debug.DebugColor.Red);
                return;
            }

            if (apiRequest?.Items == null || apiRequest.Items.Count == 0)
            {
                Debug.Print("[TryEquipCharacterItemsAsync] No items provided in the request", 0, Debug.DebugColor.Red);
                return;
            }

            // Send the request to the API
            var apiRes = await _crpgClient.UpdateCharacterEquippedItemsAsync(crpgUser.Id, crpgCharacter.Id, apiRequest);

            // Send respones to the Client
            SynchronizeUserTryEquipItemResult(networkPeer, apiRequest, apiRes);
        }
        catch (Exception ex)
        {
            Debug.Print($"[TryEquipCharacterItemsAsync] Exception for peer {networkPeer.UserName}: {ex}", 0, Debug.DebugColor.Red);
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
            Debug.Print($"[GetUserEquippedItemsAsync] No user data or character found for peer {networkPeer.UserName}", 0, Debug.DebugColor.Red);
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
                Debug.Print($"equippedItemsRes.Data was null", 0, Debug.DebugColor.Red);
            }
        }
        catch (Exception e)
        {
            Debug.Print($"Error fetching equipped items for peer {networkPeer.UserName} character {crpgUser.Character.Name}: {e}", 0, Debug.DebugColor.Red);
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
            Debug.Print($"[UpdateUserItemsAsync] No user data found for peer {networkPeer.UserName}", 0, Debug.DebugColor.Red);
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
                Debug.Print($"Failed to fetch items for user {crpgUser.Id}", 0, Debug.DebugColor.Red);
            }
        }
        catch (Exception e)
        {
            Debug.Print($"Error fetching items for peer {networkPeer.UserName}: {e}", 0, Debug.DebugColor.Red);
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
                Debug.Print("networkPeer or items is null", 0, Debug.DebugColor.Red);
                return;
            }

            Debug.Print($"Sending {items.Count} items to peer {networkPeer.Index}");

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerSendUserInventoryItems { Items = items });
            GameNetwork.EndModuleEventAsServer();
        }
        catch (Exception ex)
        {
            Debug.Print($"SynchronizeUserInventoryItemsToPeer() exception: {ex}", 0, Debug.DebugColor.Red);
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
            Debug.Print($"SynchronizeUserCharacterEquippedItemsToPeer() exception: {ex}", 0, Debug.DebugColor.Red);
            Console.WriteLine($"SynchronizeUserCharacterEquippedItemsToPeer() exception: {ex}");
        }
    }

    /// <summary>
    /// Sends a CrpgCharacter to the specific client.
    /// </summary>
    private void SynchronizeUserCharacterBasicToPeer(NetworkCommunicator networkPeer, CrpgCharacter crpgCharacter)
    {
        try
        {
            Debug.Print("SynchronizeUserCharacterEquippedItemsToPeer()");

            if (networkPeer == null || crpgCharacter == null)
            {
                Debug.Print("networkPeer or crpgCharacter is null", 0, Debug.DebugColor.Red);
                return;
            }

            Debug.Print($"Sending Character basic ({crpgCharacter.Name}) to peer {networkPeer.Index}");

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerSendUserCharacterBasic { Character = crpgCharacter });
            GameNetwork.EndModuleEventAsServer();

            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (crpgPeer?.User != null && crpgPeer?.User?.Character != null)
            {
                Debug.Print($"Sending Character statistics also.");
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerSendUserCharacterStatistics { CharacterStatistics = crpgPeer.User.Character.Statistics });
                GameNetwork.EndModuleEventAsServer();
            }
        }
        catch (Exception ex)
        {
            Debug.Print($"SynchronizeUserCharacterEquippedItemsToPeer() exception: {ex}", 0, Debug.DebugColor.Red);
            Console.WriteLine($"SynchronizeUserCharacterEquippedItemsToPeer() exception: {ex}");
        }
    }

    /// <summary>
    /// Sends the result of convert characteristics API request to the specific client.
    /// </summary>
    private void SynchronizeUserTryConverCharacteristicsResult(NetworkCommunicator networkPeer, CrpgGameCharacteristicConversionRequest crpgCharacter,
         CrpgResult<CrpgCharacterCharacteristics>? apiRes)
    {
        if (apiRes == null)
        {
            return;
        }

        string newErrorMessage = string.Empty;
        bool wasSuccess = true;

        if (apiRes.Errors == null || apiRes.Errors.Count == 0)
        {
            Debug.Print("Conversion Result Characterists recieved on server.");

            if (apiRes.Data == null)
            {
                Debug.Print("Characteristics Data was null", 0, Debug.DebugColor.Red);
                apiRes.Data = new CrpgCharacterCharacteristics(); // create safe default
                wasSuccess = false;
                newErrorMessage = "apiRes Data was null";
            }

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerSendConvertCharacteristicsResult
            {
                Success = wasSuccess,
                AttributesPoints = apiRes.Data.Attributes.Points,
                SkillPoints = apiRes.Data.Skills.Points,
                ErrorMessage = newErrorMessage,
            });
            GameNetwork.EndModuleEventAsServer();
            Debug.Print("Converted AttributePoints and SkillPoints sent to client");
        }
        else
        {
            // iterate safely
            if (apiRes.Errors != null)
            {
                foreach (var error in apiRes.Errors)
                {
                    Debug.Print($"Converting points of Characteristics failed. Error: {error?.Detail}", 0, Debug.DebugColor.Red);
                }

                var firstError = apiRes.Errors.FirstOrDefault();
                string errorMessage = firstError?.Detail ?? "Converting points of Characteristics failed";

                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerSendConvertCharacteristicsResult
                {
                    Success = wasSuccess,
                    AttributesPoints = -1, // -1 will mean failed i guess
                    SkillPoints = -1,
                    ErrorMessage = errorMessage,
                });
                GameNetwork.EndModuleEventAsServer();
            }
        }
    }

    /// <summary>
    /// Sends the reult of the update characteristics API request to the specified client.
    /// </summary>
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
                Debug.Print("Characteristics Data was null", 0, Debug.DebugColor.Red);
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
                    Debug.Print($"Characteristics failed to update. Error: {error?.Detail}", 0, Debug.DebugColor.Red);
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
                Debug.Print($"Slot {failedSlot} failed to update. Error: {error.Detail}", 0, Debug.DebugColor.Red);
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

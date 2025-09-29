using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network.Armory;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgClanArmoryServer : MissionNetwork
{
    private readonly ICrpgClient _crpgClient;

    public CrpgClanArmoryServer(ICrpgClient crpgClient)
    {
        _crpgClient = crpgClient;
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        Debug.Print("CrpgClanArmoryServer behavior initialized.", 0, Debug.DebugColor.Yellow);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<UserRequestClanArmoryAction>(HandleUserRequestClanArmoryAction);
    }

    private static ServerSendArmoryActionResult CreateDefaultResult(ClanArmoryActionRequest request)
    {
        return new ServerSendArmoryActionResult
        {
            ActionType = request.ActionType,
            ClanId = request.ClanId,
            UserItemId = request.UserItemId,
            UserId = request.UserId,
            Success = false,
            ErrorMessage = string.Empty,
            ArmoryItems = new List<CrpgClanArmoryItem>(),
        };
    }

    private bool HandleUserRequestClanArmoryAction(NetworkCommunicator peer, UserRequestClanArmoryAction message)
    {
        if (message == null)
        {
            return false;
        }

        _ = HandleUserRequestClanArmoryActionAsync(peer, message)
            .ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Debug.Print($"[ArmoryServer] Task failed: {message.Request.ActionType}", 0, Debug.DebugColor.Red);
                    Debug.Print($"{t.Exception}", 0, Debug.DebugColor.Red);
                    Debug.Print($"{t.Exception.Message}", 0, Debug.DebugColor.Red);
                }
            });
        return true;
    }

    private async Task HandleUserRequestClanArmoryActionAsync(NetworkCommunicator peer, UserRequestClanArmoryAction message)
    {
        var request = message.Request;

        var result = CreateDefaultResult(request);

        try
        {
            switch (request.ActionType)
            {
                case ClanArmoryActionType.Add:
                    await HandleAddAsync(request, result);
                    break;

                case ClanArmoryActionType.Remove:
                    await HandleRemoveAsync(request, result);
                    break;

                case ClanArmoryActionType.Borrow:
                    await HandleBorrowAsync(request, result);
                    break;

                case ClanArmoryActionType.Return:
                    await HandleReturnAsync(request, result);
                    break;

                case ClanArmoryActionType.Get:
                    await HandleGetAsync(peer, request, result);
                    break;

                default:
                    result.ErrorMessage = "Unknown action type.";
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.Print($"[ArmoryServer] Exception: {ex}", 0, Debug.DebugColor.Red);
            result.ErrorMessage = ex.Message;
        }

        // Always reply to the requesting peer with result
        GameNetwork.BeginModuleEventAsServer(peer);
        GameNetwork.WriteMessage(result);
        GameNetwork.EndModuleEventAsServer();
    }

    private async Task HandleAddAsync(ClanArmoryActionRequest request, ServerSendArmoryActionResult result)
    {
        var res = await _crpgClient.ClanArmoryAddItemAsync(request.ClanId,
            new CrpgGameClanArmoryAddItemRequest { UserItemId = request.UserItemId, UserId = request.UserId });

        if (res?.Errors?.Any() == true)
        {
            result.ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Detail));
            Debug.Print($"CrpgClanArmoryServer.HandleAddAsync() error: {result.ErrorMessage}", 0, Debug.DebugColor.Red);
        }
        else
        {
            if (res?.Data != null)
            {
                result.ArmoryItems.Add(res.Data);
                BroadcastClanArmoryUpdate(request.ClanId, ClanArmoryActionType.Add, res.Data);
            }

            result.Success = true;
        }
    }

    private async Task HandleRemoveAsync(ClanArmoryActionRequest request, ServerSendArmoryActionResult result)
    {
        var res = await _crpgClient.RemoveClanArmoryItemAsync(request.ClanId, request.UserItemId, request.UserId);
        if (res?.Errors?.Any() == true)
        {
            Debug.Print($"CrpgClanArmoryServer.HandleRemoveAsync() error: {result.ErrorMessage}", 0, Debug.DebugColor.Red);
            result.ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Detail));
        }
        else
        {
            result.Success = true;
            BroadcastClanArmoryUpdate(request.ClanId, ClanArmoryActionType.Remove,
                new CrpgClanArmoryItem { UserItem = new CrpgUserItemExtended { Id = request.UserItemId } });
        }
    }

    private async Task HandleBorrowAsync(ClanArmoryActionRequest request, ServerSendArmoryActionResult result)
    {
        var res = await _crpgClient.ClanArmoryBorrowItemAsync(request.ClanId, request.UserItemId,
            new CrpgGameBorrowClanArmoryItemRequest { UserId = request.UserId });

        if (res?.Errors?.Any() == true)
        {
            result.ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Detail));
            Debug.Print($"CrpgClanArmoryServer.HandleBorrowAsync() error: {result.ErrorMessage}", 0, Debug.DebugColor.Red);
        }
        else
        {
            result.Success = true;
            BroadcastClanArmoryUpdate(request.ClanId, ClanArmoryActionType.Borrow,
                new CrpgClanArmoryItem { UserItem = new CrpgUserItemExtended { Id = request.UserItemId } });
        }
    }

    private async Task HandleReturnAsync(ClanArmoryActionRequest request, ServerSendArmoryActionResult result)
    {
        var res = await _crpgClient.ClanArmoryReturnItemAsync(request.ClanId, request.UserItemId,
            new CrpgGameBorrowClanArmoryItemRequest { UserId = request.UserId });

        if (res?.Errors?.Any() == true)
        {
            result.ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Detail));
            Debug.Print($"CrpgClanArmoryServer.HandleReturnAsync() error: {result.ErrorMessage}", 0, Debug.DebugColor.Red);
        }
        else
        {
            result.Success = true;
            BroadcastClanArmoryUpdate(request.ClanId, ClanArmoryActionType.Return,
                new CrpgClanArmoryItem { UserItem = new CrpgUserItemExtended { Id = request.UserItemId } });
        }
    }

    private async Task HandleGetAsync(NetworkCommunicator peer, ClanArmoryActionRequest request, ServerSendArmoryActionResult result)
    {
        var res = await _crpgClient.GetClanArmoryAsync(request.ClanId, request.UserId);
        if (res?.Errors?.Any() == true)
        {
            result.ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Detail));
        }
        else if (res?.Data != null)
        {
            result.Success = true;
            result.ArmoryItems = res.Data.ToList();

            // 'Get' only replies to requester, does not broadcast.
            Debug.Print("Sending ServerSendClanArmoryCompleteUpdate()", 0, Debug.DebugColor.Yellow);
            GameNetwork.BeginModuleEventAsServer(peer);
            GameNetwork.WriteMessage(new ServerSendClanArmoryCompleteUpdate
            {
                ArmoryItems = result.ArmoryItems,
            });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    private void BroadcastClanArmoryUpdate(int clanId, ClanArmoryActionType action, CrpgClanArmoryItem item)
    {
        foreach (var otherPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = otherPeer.GetComponent<CrpgPeer>();
            if (crpgPeer?.Clan?.Id == clanId)
            {
                GameNetwork.BeginModuleEventAsServer(otherPeer);
                GameNetwork.WriteMessage(new ServerSendClanArmoryItemUpdate
                {
                    ActionType = action,
                    ArmoryItem = item,
                });
                GameNetwork.EndModuleEventAsServer();
            }
        }
    }
}

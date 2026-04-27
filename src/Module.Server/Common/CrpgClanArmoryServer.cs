using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network.Armory;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgClanArmoryServer(ICrpgClient crpgClient) : MissionNetwork
{
    private readonly ICrpgClient _crpgClient = crpgClient;

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<UserRequestClanArmoryAction>(HandleUserRequestClanArmoryAction);
    }

    private bool HandleUserRequestClanArmoryAction(NetworkCommunicator peer, UserRequestClanArmoryAction message)
    {
        if (message == null)
        {
            return false;
        }

        _ = HandleUserRequestClanArmoryActionAsync(peer, message).ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                Debug.Print($"[ArmoryServer] Task failed: {message.Request.ActionType} {t.Exception.InnerException?.Message}", 0, Debug.DebugColor.Red);
            }
        });
        return true;
    }

    private async Task HandleUserRequestClanArmoryActionAsync(NetworkCommunicator peer, UserRequestClanArmoryAction message)
    {
        var sync = SynchronizationContext.Current;
        var request = message.Request;
        request.UserId = peer.GetComponent<CrpgPeer>()?.User?.Id ?? -1;

        var result = new ServerSendArmoryActionResult
        {
            ActionType = request.ActionType,
            ClanId = request.ClanId,
            UserItemId = request.UserItemId,
            UserId = request.UserId,
            Success = false,
            ErrorMessage = string.Empty,
            ArmoryItems = [],
        };

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
                    await HandleGetAsync(peer, request, result, sync);
                    return; // Get sends its own response
                default:
                    result.ErrorMessage = "Unknown action type.";
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.Print($"[ArmoryServer] ExL {ex} Exception: {ex.Message}", 0, Debug.DebugColor.Red);
            result.ErrorMessage = ex.ToString();
        }

        SendResult(peer, result, sync);
    }

    private void SendResult(NetworkCommunicator peer, ServerSendArmoryActionResult result, SynchronizationContext? sync)
    {
        void Send()
        {
            if (!peer.IsConnectionActive || !peer.IsSynchronized)
            {
                return;
            }

            GameNetwork.BeginModuleEventAsServer(peer);
            GameNetwork.WriteMessage(result);
            GameNetwork.EndModuleEventAsServer();
        }

        if (sync != null)
        {
            sync.Post(_ => Send(), null);
        }
        else
        {
            Send();
        }
    }

    private async Task HandleAddAsync(ClanArmoryActionRequest request, ServerSendArmoryActionResult result)
    {
        var res = await _crpgClient.ClanArmoryAddItemAsync(request.ClanId,
            new CrpgGameClanArmoryAddItemRequest { UserItemId = request.UserItemId, UserId = request.UserId });

        if (res?.Errors?.Any() == true)
        {
            result.ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Detail));
            return;
        }

        if (res?.Data != null)
        {
            result.ArmoryItems.Add(res.Data);
            BroadcastClanArmoryUpdate(request.ClanId, ClanArmoryActionType.Add, res.Data);
        }

        result.Success = true;
    }

    private async Task HandleRemoveAsync(ClanArmoryActionRequest request, ServerSendArmoryActionResult result)
    {
        var res = await _crpgClient.RemoveClanArmoryItemAsync(request.ClanId, request.UserItemId, request.UserId);
        if (res?.Errors?.Any() == true)
        {
            result.ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Detail));
            return;
        }

        result.Success = true;
        BroadcastClanArmoryUpdate(request.ClanId, ClanArmoryActionType.Remove,
            new CrpgClanArmoryItem { UserItem = new CrpgUserItemExtended { Id = request.UserItemId } });
    }

    private async Task HandleBorrowAsync(ClanArmoryActionRequest request, ServerSendArmoryActionResult result)
    {
        var res = await _crpgClient.ClanArmoryBorrowItemAsync(request.ClanId, request.UserItemId,
            new CrpgGameBorrowClanArmoryItemRequest { UserId = request.UserId });

        if (res?.Errors?.Any() == true)
        {
            result.ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Detail));
            return;
        }

        result.Success = true;
        BroadcastClanArmoryUpdate(request.ClanId, ClanArmoryActionType.Borrow,
            new CrpgClanArmoryItem { UserItem = new CrpgUserItemExtended { Id = request.UserItemId } });
    }

    private async Task HandleReturnAsync(ClanArmoryActionRequest request, ServerSendArmoryActionResult result)
    {
        var res = await _crpgClient.ClanArmoryReturnItemAsync(request.ClanId, request.UserItemId,
            new CrpgGameBorrowClanArmoryItemRequest { UserId = request.UserId });

        if (res?.Errors?.Any() == true)
        {
            result.ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Detail));
            return;
        }

        result.Success = true;
        BroadcastClanArmoryUpdate(request.ClanId, ClanArmoryActionType.Return,
            new CrpgClanArmoryItem { UserItem = new CrpgUserItemExtended { Id = request.UserItemId } });
    }

    private async Task HandleGetAsync(NetworkCommunicator peer, ClanArmoryActionRequest request, ServerSendArmoryActionResult result, SynchronizationContext? sync)
    {
        var res = await _crpgClient.GetClanArmoryAsync(request.ClanId, request.UserId);
        if (res?.Errors?.Any() == true)
        {
            result.ErrorMessage = string.Join("; ", res.Errors.Select(e => e.Detail));
            return;
        }

        if (res?.Data == null)
        {
            return;
        }

        void Send()
        {
            try
            {
                if (!peer.IsConnectionActive || !peer.IsSynchronized)
                {
                    return;
                }

                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new ServerSendClanArmoryCompleteUpdate { ArmoryItems = res.Data.ToList() });
                GameNetwork.EndModuleEventAsServer();
            }
            catch (Exception ex)
            {
                Debug.Print($"[ArmoryServer] Send failed: {ex.Message}", 0, Debug.DebugColor.Red);
            }
        }

        if (sync != null)
        {
            sync.Post(_ => Send(), null);
        }
        else
        {
            Send();
        }
    }

    private void BroadcastClanArmoryUpdate(int clanId, ClanArmoryActionType action, CrpgClanArmoryItem item)
    {
        foreach (var otherPeer in GameNetwork.NetworkPeers)
        {
            if (otherPeer.GetComponent<CrpgPeer>()?.Clan?.Id == clanId && otherPeer.IsConnectionActive)
            {
                GameNetwork.BeginModuleEventAsServer(otherPeer);
                GameNetwork.WriteMessage(new ServerSendClanArmoryItemUpdate { ActionType = action, ArmoryItem = item });
                GameNetwork.EndModuleEventAsServer();
            }
        }
    }
}

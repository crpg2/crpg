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

                var items = res.Data.ToList();

                using System.IO.MemoryStream rawStream = new();
                using (var writer = new System.IO.BinaryWriter(rawStream, System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write(request.ClanId);
                    writer.Write(items.Count);
                    foreach (var item in items)
                    {
                        writer.Write(item.UserItem != null);
                        if (item.UserItem != null)
                        {
                            writer.Write(item.UserItem.Id);
                            writer.Write(item.UserItem.UserId);
                            writer.Write(item.UserItem.Rank);
                            writer.Write(item.UserItem.ItemId ?? string.Empty);
                            writer.Write(item.UserItem.IsBroken);
                            writer.Write(item.UserItem.IsArmoryItem);
                            writer.Write(item.UserItem.IsPersonal);
                            writer.Write(item.UserItem.CreatedAt.ToBinary());
                        }

                        writer.Write(item.BorrowedItem != null);
                        if (item.BorrowedItem != null)
                        {
                            writer.Write(item.BorrowedItem.BorrowerUserId);
                            writer.Write(item.BorrowedItem.UserItemId);
                            writer.Write(item.BorrowedItem.UpdatedAt.ToBinary());
                        }

                        writer.Write(item.UpdatedAt.ToBinary());
                    }
                }

                using System.IO.MemoryStream compressedStream = new();
                rawStream.Position = 0;
                using (var gzip = new System.IO.Compression.GZipStream(compressedStream, System.IO.Compression.CompressionMode.Compress, leaveOpen: true))
                {
                    rawStream.CopyTo(gzip);
                }

                byte[] compressed = compressedStream.ToArray();
                const int chunkSize = 500;
                int total = compressed.Length;

                for (int offset = 0; offset < total; offset += chunkSize)
                {
                    int length = Math.Min(chunkSize, total - offset);
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new ServerSendClanArmoryCompleteUpdate
                    {
                        IsFirstChunk = offset == 0,
                        IsLastChunk = offset + length >= total,
                        ChunkData = compressed[offset..(offset + length)],
                    });
                    GameNetwork.EndModuleEventAsServer();
                }
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
        foreach (var peer in GameNetwork.NetworkPeers)
        {
            if (peer.GetComponent<CrpgPeer>()?.Clan?.Id == clanId && peer.IsConnectionActive && peer.IsSynchronized)
            {
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new ServerSendClanArmoryItemUpdate { ActionType = action, ArmoryItem = item });
                GameNetwork.EndModuleEventAsServer();
            }
        }
    }
}

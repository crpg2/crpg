using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network.CharacterLoadout;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common;

internal class CrpgCharacterLoadoutBehaviorServer(ICrpgClient crpgClient, CrpgGameMode crpgGameMode) : MissionNetwork
{
    private readonly Dictionary<NetworkCommunicator, List<CrpgEquippedItemExtended>> _peerEquippedItems = [];
    private MissionLobbyComponent? _lobbyComponent;
    public bool IsEnabled { get; private set; }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        SetEnabled(CrpgServerConfiguration.IsCharacterLoadoutEnabled);
        CrpgServerConfiguration.OnCrpgServerConfigurationChanged += HandleCrpgServerConfigurationChanged;
        CrpgUserManagerServer.OnCrpgUserLoaded += HandleCrpgUserLoaded;
        _lobbyComponent = Mission.Current.GetMissionBehavior<MissionLobbyComponent>();
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        CrpgServerConfiguration.OnCrpgServerConfigurationChanged -= HandleCrpgServerConfigurationChanged;
        CrpgUserManagerServer.OnCrpgUserLoaded -= HandleCrpgUserLoaded;
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);

        if (!IsEnabled || !agent.IsPlayerControlled)
        {
            return;
        }

        NetworkCommunicator? peer = agent.MissionPeer?.GetNetworkPeer();
        if (peer == null)
        {
            return;
        }

        if (_lobbyComponent?.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.Playing)
        {
            ReadyToSpawn.Remove(peer);
        }

        // UnsetReadyToSpawnShowMenu(peer, "Confirm your equipment");
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

        if (!IsEnabled || !affectedAgent.IsPlayerControlled)
        {
            return;
        }

        NetworkCommunicator? peer = affectedAgent.MissionPeer?.GetNetworkPeer();
        if (peer == null || !peer.IsConnectionActive)
        {
            return;
        }

        if (_lobbyComponent?.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.Playing)
        {
            ReadyToSpawn.Add(peer);
        }
    }

    public override void OnPlayerDisconnectedFromServer(NetworkCommunicator peer)
    {
        base.OnPlayerDisconnectedFromServer(peer);
        ReadyToSpawn.Remove(peer);
        _peerEquippedItems.Remove(peer);
    }

    internal HashSet<NetworkCommunicator> ReadyToSpawn { get; private set; } = [];
    internal IReadOnlyList<CrpgEquippedItemExtended> GetPeerEquippedItems(NetworkCommunicator peer)
    {
        return _peerEquippedItems.TryGetValue(peer, out var items)
            ? items.AsReadOnly()
            : Array.Empty<CrpgEquippedItemExtended>();
    }

    internal void UnsetReadyToSpawnShowMenu(NetworkCommunicator peer, string reason = "")
    {
        ReadyToSpawn.Remove(peer);
        SendMessageToPeer(peer, new ServerSendForceEquipMenu { Show = true, Msg = reason });
    }

    /// <summary>
    /// Returns the equipment built from the server-side equipped items cache for this peer.
    /// Returns an empty <see cref="Equipment"/> if the cache has not yet been populated
    /// (i.e. the async load from the API is still in flight).
    /// </summary>
    internal Equipment GetPeerEquipment(NetworkCommunicator peer)
    {
        var equippedItems = GetPeerEquippedItems(peer)
            .Select(e => new CrpgEquippedItem
            {
                Slot = e.Slot,
                UserItem = new CrpgUserItem { Id = e.UserItem?.Id ?? -1, ItemId = e.UserItem?.ItemId ?? string.Empty },
            })
            .ToList();

        return CrpgCharacterBuilder.CreateCharacterEquipment(equippedItems);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<UserRequestGetInventoryItems>(HandleUserRequestGetInventoryItems);
        registerer.Register<UserRequestGetEquippedItems>(HandleUserRequestGetEquippedItems);
        registerer.Register<UserRequestEquipCharacterItem>(HandleUserRequestEquipCharacterItem);
        registerer.Register<UserRequestGetCharacterBasic>(HandleUserRequestGetCharacterBasic);
        registerer.Register<UserRequestUpdateCharacterCharacteristics>(HandleUserRequestUpdateCharacterCharacteristics);
        registerer.Register<UserRequestConvertCharacteristics>(HandleUserRequestConvertCharacteristics);
        registerer.Register<UserRequestGetUserInfo>(HandleUserRequestGetUserInfo);
        registerer.Register<UserRequestReadyToSpawn>(HandleUserRequestReadyToSpawn);
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator peer)
    {
        base.HandleEarlyNewClientAfterLoadingFinished(peer);
        ReadyToSpawn.Add(peer);
        SendMessageToPeer(peer, new ServerSendUserCharacterLoadoutEnabled { IsEnabled = CrpgServerConfiguration.IsCharacterLoadoutEnabled });
    }

    private void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;

        // Tell clients
        if (!GameNetwork.NetworkPeers.Any())
        {
            return;
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new ServerSendUserCharacterLoadoutEnabled { IsEnabled = enabled });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private bool HandleUserRequestReadyToSpawn(NetworkCommunicator peer, UserRequestReadyToSpawn message)
    {
        ReadyToSpawn.Add(peer);
        return true;
    }

    private void HandleCrpgUserLoaded(NetworkCommunicator peer)
    {
        if (!IsEnabled)
        {
            return;
        }

        var user = peer.GetComponent<CrpgPeer>()?.User;
        var character = user?.Character;
        if (user == null || character == null)
        {
            return;
        }

        _ = GetUserEquippedItemsAsync(peer, user, character).ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                Debug.Print($"[LoadoutServer] GetUserEquippedItemsAsync failed: {t.Exception.InnerException?.Message}", 0, Debug.DebugColor.Red);
            }
        });
    }

    private void HandleCrpgServerConfigurationChanged(string key, object value)
    {
        switch (key)
        {
            case nameof(CrpgServerConfiguration.IsCharacterLoadoutEnabled):
                SetEnabled((bool)value);
                break;
        }
    }

    private void SendMessageToPeer(NetworkCommunicator peer, GameNetworkMessage message)
    {
        if (peer == null || !peer.IsConnectionActive)
        {
            return;
        }

        GameNetwork.BeginModuleEventAsServer(peer);
        GameNetwork.WriteMessage(message);
        GameNetwork.EndModuleEventAsServer();
    }

    private bool HandleRequest<TMessage>(
        NetworkCommunicator peer,
        TMessage message,
        CrpgUser? user,
        CrpgCharacter? character,
        Func<NetworkCommunicator, TMessage, Task> handler)
    {
        if (user == null || character == null)
        {
            Debug.Print($"[{typeof(TMessage).Name}] Rejected: user={user?.Id.ToString() ?? "null"} character={character?.Id.ToString() ?? "null"} peer={peer.UserName}", 0, Debug.DebugColor.Red);
            return false;
        }

        _ = handler(peer, message).ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                Debug.Print($"[{typeof(TMessage).Name}] Handler failed: {t.Exception.InnerException?.Message}", 0, Debug.DebugColor.Red);
            }
        });
        return true;
    }

    // user-only overload for handlers that don't need character
    private bool HandleRequest<TMessage>(
        NetworkCommunicator peer,
        TMessage message,
        CrpgUser? user,
        Func<NetworkCommunicator, TMessage, Task> handler)
    {
        if (user == null)
        {
            Debug.Print($"[{typeof(TMessage).Name}] Rejected: user=null peer={peer.UserName}", 0, Debug.DebugColor.Red);
            return false;
        }

        _ = handler(peer, message).ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                Debug.Print($"[{typeof(TMessage).Name}] Handler failed: {t.Exception.InnerException?.Message}", 0, Debug.DebugColor.Red);
            }
        });
        return true;
    }

    // handlers for results of API calls
    private bool HandleUserRequestGetInventoryItems(NetworkCommunicator peer, UserRequestGetInventoryItems message)
    {
        var user = peer.GetComponent<CrpgPeer>()?.User;
        return HandleRequest(peer, message, user, (p, _) => UpdateUserItemsAsync(p, user!));
    }

    private bool HandleUserRequestGetUserInfo(NetworkCommunicator peer, UserRequestGetUserInfo message)
    {
        var user = peer.GetComponent<CrpgPeer>()?.User;
        return HandleRequest(peer, message, user, (p, _) => GetUserInfoAsync(p, user!));
    }

    private bool HandleUserRequestGetCharacterBasic(NetworkCommunicator peer, UserRequestGetCharacterBasic message)
    {
        var user = peer.GetComponent<CrpgPeer>()?.User;
        var character = user?.Character;
        return HandleRequest(peer, message, user, character, (p, _) => GetUserCharacterBasicAsync(p, user!, character!));
    }

    private bool HandleUserRequestGetEquippedItems(NetworkCommunicator peer, UserRequestGetEquippedItems message)
    {
        var user = peer.GetComponent<CrpgPeer>()?.User;
        var character = user?.Character;
        return HandleRequest(peer, message, user, character, (p, _) => GetUserEquippedItemsAsync(p, user!, character!));
    }

    private bool HandleUserRequestEquipCharacterItem(NetworkCommunicator peer, UserRequestEquipCharacterItem message)
    {
        var user = peer.GetComponent<CrpgPeer>()?.User;
        var character = user?.Character;
        return HandleRequest(peer, message, user, character, (p, m) =>
        {
            int? userItemId = m.UserItemId == -1 ? null : m.UserItemId;
            var apiRequest = new CrpgGameCharacterItemsUpdateRequest
            {
                Items = new List<CrpgEquippedItemId> { new() { Slot = m.Slot, UserItemId = userItemId } },
            };
            return TryEquipCharacterItemsAsync(p, user!, character!, apiRequest);
        });
    }

    private bool HandleUserRequestUpdateCharacterCharacteristics(NetworkCommunicator peer, UserRequestUpdateCharacterCharacteristics message)
    {
        var user = peer.GetComponent<CrpgPeer>()?.User;
        var character = user?.Character;
        return HandleRequest(peer, message, user, character, (p, m) =>
            TryUpdateCharacterCharacteristicsAsync(p, user!, character!,
                new CrpgGameCharacterCharacteristicsUpdateRequest { Characteristics = m.Characteristics }));
    }

    private bool HandleUserRequestConvertCharacteristics(NetworkCommunicator peer, UserRequestConvertCharacteristics message)
    {
        var user = peer.GetComponent<CrpgPeer>()?.User;
        var character = user?.Character;
        return HandleRequest(peer, message, user, character, (p, m) =>
            TryConvertCharacterCharacteristicsAsync(p, user!, character!, m.ConversionRequest));
    }

    // async methods for API calls
    private async Task UpdateUserItemsAsync(NetworkCommunicator peer, CrpgUser user)
    {
        var res = await crpgClient.GetUserItemsAsync(user.Id);
        if (res.Data == null)
        {
            return;
        }

        var items = res.Data;

        using MemoryStream rawStream = new();
        using (System.IO.BinaryWriter writer = new(rawStream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(items.Count);
            foreach (var item in items)
            {
                writer.Write(item.Id);
                writer.Write(item.UserId);
                writer.Write(item.Rank);
                writer.Write(item.ItemId ?? string.Empty);
                writer.Write(item.IsBroken);
                writer.Write(item.CreatedAt.ToBinary());
                writer.Write(item.IsArmoryItem);
                writer.Write(item.IsPersonal);
            }
        }

        using MemoryStream compressedStream = new();
        rawStream.Position = 0;
        using (System.IO.Compression.GZipStream gzip = new(compressedStream, System.IO.Compression.CompressionMode.Compress, leaveOpen: true))
        {
            rawStream.CopyTo(gzip);
        }

        byte[] compressed = compressedStream.ToArray();
        const int chunkSize = 500;
        int total = compressed.Length;
        // Debug.Print($"[LoadoutServer] Inventory: {items.Count} items, {total} bytes compressed", 0, Debug.DebugColor.Cyan);

        for (int offset = 0; offset < total; offset += chunkSize)
        {
            int length = Math.Min(chunkSize, total - offset);
            bool isFirst = offset == 0;
            bool isLast = offset + length >= total;
            SendMessageToPeer(peer, new ServerSendUserInventoryItems
            {
                IsFirstChunk = isFirst,
                IsLastChunk = isLast,
                ChunkData = compressed[offset..(offset + length)],
            });
        }
    }

    private async Task GetUserCharacterBasicAsync(NetworkCommunicator peer, CrpgUser user, CrpgCharacter character)
    {
        var res = await crpgClient.GetUserCharacterBasicAsync(user.Id, character.Id);
        if (res.Data == null)
        {
            return;
        }

        SendMessageToPeer(peer, new ServerSendUserCharacterBasic { Character = res.Data });
    }

    private async Task GetUserInfoAsync(NetworkCommunicator peer, CrpgUser user)
    {
        var res = await crpgClient.GetUserAsync(user.Platform, user.PlatformUserId, user.Region, crpgGameMode);
        if (res.Data != null)
        {
            SendMessageToPeer(peer, new ServerSendUserInfo { User = res.Data });
        }
    }

    private async Task GetUserEquippedItemsAsync(NetworkCommunicator peer, CrpgUser user, CrpgCharacter character)
    {
        var res = await crpgClient.GetCharacterEquippedItemsAsync(user.Id, character.Id);
        if (res.Data == null)
        {
            return;
        }

        _peerEquippedItems[peer] = res.Data
            .Select(e => new CrpgEquippedItemExtended
            {
                Slot = e.Slot,
                UserItem = new CrpgUserItemExtended { Id = e.UserItem?.Id ?? -1, ItemId = e.UserItem?.ItemId ?? string.Empty },
            })
            .ToList();

        using MemoryStream rawStream = new();
        using (var writer = new System.IO.BinaryWriter(rawStream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(res.Data.Count);
            foreach (var item in res.Data)
            {
                writer.Write((int)item.Slot);
                writer.Write(item.UserItem?.Id ?? -1);
                writer.Write(item.UserItem?.UserId ?? -1);
                writer.Write(item.UserItem?.Rank ?? 0);
                writer.Write(item.UserItem?.ItemId ?? string.Empty);
                writer.Write(item.UserItem?.IsBroken ?? false);
                writer.Write(item.UserItem?.CreatedAt.ToBinary() ?? 0L);
                writer.Write(item.UserItem?.IsArmoryItem ?? false);
                writer.Write(item.UserItem?.IsPersonal ?? false);
            }
        }

        using MemoryStream compressedStream = new();
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
            SendMessageToPeer(peer, new ServerSendUserCharacterEquippedItems
            {
                IsFirstChunk = offset == 0,
                IsLastChunk = offset + length >= total,
                ChunkData = compressed[offset..(offset + length)],
            });
        }
    }

    private async Task TryEquipCharacterItemsAsync(NetworkCommunicator peer, CrpgUser user, CrpgCharacter character, CrpgGameCharacterItemsUpdateRequest apiRequest)
    {
        if (apiRequest.Items?.Count == 0)
        {
            return;
        }

        var res = await crpgClient.UpdateCharacterEquippedItemsAsync(user.Id, character.Id, apiRequest);

        if (res.Errors?.Count > 0)
        {
            SendMessageToPeer(peer, new ServerSendEquipItemResult
            {
                Success = false,
                SlotIndex = (int)apiRequest.Items![0].Slot,
                UserItemId = -1,
                ErrorMessage = res.Errors[0].Detail ?? "Failed to equip item",
            });
            return;
        }

        foreach (var requestedItem in apiRequest.Items!)
        {
            var updatedItem = res.Data?.FirstOrDefault(u => u.Slot == requestedItem.Slot);

            if (_peerEquippedItems.TryGetValue(peer, out var equippedItems))
            {
                equippedItems.RemoveAll(e => e.Slot == requestedItem.Slot);
                if (updatedItem != null)
                {
                    equippedItems.Add(new CrpgEquippedItemExtended
                    {
                        Slot = updatedItem.Slot,
                        UserItem = new CrpgUserItemExtended
                        {
                            Id = updatedItem.UserItem?.Id ?? -1,
                            ItemId = updatedItem.UserItem?.ItemId ?? string.Empty,
                        },
                    });
                }
            }

            SendMessageToPeer(peer, new ServerSendEquipItemResult
            {
                Success = true,
                SlotIndex = (int)requestedItem.Slot,
                UserItemId = updatedItem?.UserItem?.Id ?? -1,
            });
        }
    }

    private async Task TryUpdateCharacterCharacteristicsAsync(NetworkCommunicator peer, CrpgUser user, CrpgCharacter character, CrpgGameCharacterCharacteristicsUpdateRequest apiRequest)
    {
        if (apiRequest.Characteristics == null)
        {
            return;
        }

        var res = await crpgClient.UpdateCharacterCharacteristicsAsync(user.Id, character.Id, apiRequest);

        bool success = res.Errors == null || res.Errors.Count == 0;
        Debug.Print($"TryUpdateCharacterCharacteristics: success={success} errors={res.Errors?.Count ?? -1} data={res.Data != null} firstError={res.Errors?.FirstOrDefault()?.Detail ?? "none"}");

        SendMessageToPeer(peer, new ServerSendUpdateCharacteristicsResult
        {
            Success = success,
            Characteristics = res.Data ?? new CrpgCharacterCharacteristics(),
            ErrorMessage = res.Errors?.FirstOrDefault()?.Detail ?? string.Empty,
        });
    }

    private async Task TryConvertCharacterCharacteristicsAsync(NetworkCommunicator peer, CrpgUser user, CrpgCharacter character, CrpgGameCharacteristicConversionRequest apiRequest)
    {
        var res = await crpgClient.ConvertCharacterCharacteristicsAsync(user.Id, character.Id, apiRequest);
        bool success = (res.Errors == null || res.Errors.Count == 0) && res.Data != null;

        SendMessageToPeer(peer, new ServerSendConvertCharacteristicsResult
        {
            Success = success,
            AttributesPoints = res.Data?.Attributes.Points ?? -1,
            SkillPoints = res.Data?.Skills.Points ?? -1,
            ErrorMessage = res.Errors?.FirstOrDefault()?.Detail ?? string.Empty,
        });
    }
}

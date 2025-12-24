using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network.Armory;
using Crpg.Module.GUI.Inventory;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgClanArmoryClient : MissionNetwork
{
    private readonly List<CrpgClanArmoryItem> _clanArmoryItems = [];
    private readonly TimeSpan _requestCooldown = TimeSpan.FromSeconds(2);
    private DateTime _lastRequestTime = DateTime.MinValue;
    private bool _hasReceivedFullUpdate;
    private bool _outstandingRequest;

    public IReadOnlyList<CrpgClanArmoryItem> ClanArmoryItems => _clanArmoryItems;
    public int UserClanId { get; private set; } = -1;

    internal event Action? OnClanArmoryUpdated;
    internal event Action<ClanArmoryActionType, int>? OnArmoryActionUpdated;

    internal void RequestArmoryAction(ClanArmoryActionType action, CrpgUserItemExtended? uItem)
    {
        if (uItem == null && action != ClanArmoryActionType.Get)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (now - _lastRequestTime < _requestCooldown || _outstandingRequest)
        {
            CrpgCharacterEquipVM.RequestStatusMessage(
                new TextObject("{=KC9dx226}Armory -- please wait before trying again.").ToString(), isError: true);

            return;
        }

        var crpgPeer = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.GetComponent<CrpgPeer>();
        UserClanId = crpgPeer?.Clan?.Id ?? -1;
        int userId = crpgPeer?.User?.Id ?? -1;

        if (UserClanId < 0 || userId < 0)
        {
            Debug.Print("RequestArmoryAction: clanId or userId is invalid", 0, Debug.DebugColor.Red);
            return;
        }

        _lastRequestTime = now;
        _outstandingRequest = true;

        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestClanArmoryAction
        {
            Request = new ClanArmoryActionRequest
            {
                ActionType = action,
                ClanId = UserClanId,
                UserId = userId,
                UserItemId = uItem?.Id ?? -1,
            },
        });
        GameNetwork.EndModuleEventAsClient();
    }

    internal void SetClanArmoryItems(IEnumerable<CrpgClanArmoryItem> items)
    {
        _clanArmoryItems.Clear();
        _clanArmoryItems.AddRange(items);
        _hasReceivedFullUpdate = true;
        OnClanArmoryUpdated?.Invoke();
    }

    internal bool GetCrpgUserItemArmoryStatus(int userItemId, out CrpgGameArmoryItemStatus armoryStatus)
    {
        armoryStatus = 0;
        int? myUserId = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.GetComponent<CrpgPeer>()?.User?.Id;

        var item = FindArmoryItem(userItemId);
        if (item == null)
        {
            return false;
        }

        bool isOwner = item.UserItem?.UserId == myUserId;
        int? borrowerId = item.BorrowedItem?.BorrowerUserId;
        bool isBorrowed = borrowerId is not null;
        bool isBorrower = borrowerId == myUserId;

        armoryStatus = isOwner
            ? (isBorrowed ? CrpgGameArmoryItemStatus.YoursBorrowed : CrpgGameArmoryItemStatus.YoursAvailable)
            : isBorrower
                ? CrpgGameArmoryItemStatus.BorrowedByYou
                : (isBorrowed ? CrpgGameArmoryItemStatus.NotYoursBorrowed : CrpgGameArmoryItemStatus.NotYoursAvailable);

        return true;
    }

    internal CrpgClanArmoryItem? FindArmoryItem(int userItemId) =>
        _clanArmoryItems.FirstOrDefault(x => x.UserItem?.Id == userItemId || x.BorrowedItem?.UserItemId == userItemId);

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<ServerSendArmoryActionResult>(HandleArmoryActionResult);
        registerer.Register<ServerSendClanArmoryItemUpdate>(HandleClanArmoryItemUpdate);
        registerer.Register<ServerSendClanArmoryCompleteUpdate>(HandleClanArmoryCompleteUpdate);
    }

    private void HandleArmoryActionResult(ServerSendArmoryActionResult message)
    {
        _outstandingRequest = false;
        if (!message.Success)
        {
            Debug.Print($"ArmoryActionResult: ({message.ActionType}) failed. Error: {message.ErrorMessage}", 0, Debug.DebugColor.Red);
            CrpgCharacterEquipVM.RequestStatusMessage(new TextObject("{=KC9dx227}Armory action failed: {ERROR}")
                .SetTextVariable("ERROR", message.ErrorMessage).ToString(), isError: true);
        }
    }

    private void HandleClanArmoryItemUpdate(ServerSendClanArmoryItemUpdate message)
    {
        int userItemId = message.ArmoryItem?.UserItem?.Id ?? -1;
        if (userItemId < 0 || !_hasReceivedFullUpdate)
        {
            return;
        }

        var existingItem = _clanArmoryItems.FirstOrDefault(x => x.UserItem?.Id == userItemId);

        switch (message.ActionType)
        {
            case ClanArmoryActionType.Add:
                if (existingItem == null && message.ArmoryItem != null)
                {
                    _clanArmoryItems.Add(message.ArmoryItem);
                }
                else if (existingItem?.UserItem != null)
                {
                    existingItem.UserItem.IsArmoryItem = true;
                }

                break;

            case ClanArmoryActionType.Remove:
                if (existingItem != null)
                {
                    _clanArmoryItems.Remove(existingItem);
                }

                break;

            case ClanArmoryActionType.Borrow:
                if (existingItem?.UserItem != null)
                {
                    existingItem.UserItem.IsArmoryItem = true;
                    existingItem.BorrowedItem = new CrpgClanArmoryBorrowedItem
                    {
                        BorrowerUserId = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.GetComponent<CrpgPeer>()?.User?.Id ?? -1,
                        UserItemId = existingItem.UserItem.Id,
                        UpdatedAt = message.ArmoryItem?.UpdatedAt ?? DateTime.UtcNow,
                    };
                    existingItem.UpdatedAt = message.ArmoryItem?.UpdatedAt ?? DateTime.UtcNow;
                }

                break;

            case ClanArmoryActionType.Return:
                if (existingItem != null && message.ArmoryItem != null)
                {
                    if (existingItem.UserItem != null)
                    {
                        existingItem.UserItem.IsArmoryItem = true;
                    }

                    existingItem.BorrowedItem = null;
                    existingItem.UpdatedAt = message.ArmoryItem.UpdatedAt;
                }
                else
                {
                    Debug.Print("HandleClanArmoryItemUpdate Return: existingItem or message.ArmoryItem is null", 0, Debug.DebugColor.Red);
                }

                break;
        }

        OnArmoryActionUpdated?.Invoke(message.ActionType, userItemId);
    }

    private void HandleClanArmoryCompleteUpdate(ServerSendClanArmoryCompleteUpdate message)
    {
        _outstandingRequest = false;
        SetClanArmoryItems(message.ArmoryItems);
    }
}

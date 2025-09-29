using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network.Armory;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgClanArmoryClient : MissionNetwork
{
    private readonly List<CrpgClanArmoryItem> _clanArmoryItems = new();
    private MissionNetworkComponent? _missionNetworkComponent;

    public IReadOnlyList<CrpgClanArmoryItem> ClanArmoryItems => _clanArmoryItems;

    internal event Action? OnClanArmoryUpdated;
    internal event Action<ClanArmoryActionType, int>? OnArmoryActionUpdated;

    public CrpgClanArmoryClient()
    {
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _missionNetworkComponent = Mission.GetMissionBehavior<MissionNetworkComponent>();
        if (_missionNetworkComponent != null)
        {
            // _missionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
            RequestArmoryAction(ClanArmoryActionType.Get, null);
        }
    }

    internal void RequestArmoryAction(ClanArmoryActionType action, CrpgUserItemExtended? uItem)
    {
        if (uItem == null && action != ClanArmoryActionType.Get)
        {
            return;
        }

        int clanId = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.GetComponent<CrpgPeer>()?.Clan?.Id ?? -1;
        int userId = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.GetComponent<CrpgPeer>()?.User?.Id ?? -1;

        if (clanId < 0 || userId < 0)
        {
            LogDebugError("RequestArmoryAction clanId or userId is invalid");
            return;
        }

        var request = new ClanArmoryActionRequest
        {
            ActionType = action,
            ClanId = clanId,
            UserId = userId,
            UserItemId = uItem?.Id ?? -1,
        };

        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestClanArmoryAction
        {
            Request = request,
        });
        GameNetwork.EndModuleEventAsClient();
    }

    internal void SetClanArmoryItems(IEnumerable<CrpgClanArmoryItem> items)
    {
        _clanArmoryItems.Clear();
        _clanArmoryItems.AddRange(items);
        OnClanArmoryUpdated?.Invoke();
    }

    internal bool GetCrpgUserItemArmoryStatus(int userItemId, out CrpgGameArmoryItemStatus armoryStatus)
    {
        armoryStatus = 0;

        int? currentUserId = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.GetComponent<CrpgPeer>()?.User?.Id;

        var clanArmoryItem = FindClanArmoryItemByUserItemId(userItemId);
        if (clanArmoryItem is null)
        {
            return false;
        }

        bool isOwner = clanArmoryItem.UserItem?.UserId == currentUserId;
        int? borrowerId = clanArmoryItem.BorrowedItem?.BorrowerUserId;
        bool isBorrowed = borrowerId is not null;
        bool isBorrower = borrowerId == currentUserId;

        if (isOwner)
        {
            armoryStatus = isBorrowed
                ? CrpgGameArmoryItemStatus.YoursBorrowed
                : CrpgGameArmoryItemStatus.YoursAvailable;
            return true;
        }

        if (isBorrower)
        {
            armoryStatus = CrpgGameArmoryItemStatus.BorrowedByYou;
            return true;
        }

        armoryStatus = isBorrowed
            ? CrpgGameArmoryItemStatus.NotYoursBorrowed
            : CrpgGameArmoryItemStatus.NotYoursAvailible;
        return true;
    }

    internal CrpgClanArmoryItem? FindClanArmoryItemByUserItemId(int uItemId)
    {
        // Defensive: return null if no armory items exist
        if (_clanArmoryItems == null)
        {
            return null;
        }

        foreach (var item in _clanArmoryItems)
        {
            // Skip invalid entries
            if (item == null)
            {
                continue;
            }

            // Skip if this armory item has no UserItem assigned
            if (item.UserItem == null)
            {
                continue;
            }

            // Check if IDs match
            if (item.UserItem.Id == uItemId)
            {
                return item; // Found a match
            }

            // Check borrowed item
            if (item.BorrowedItem?.UserItemId == uItemId)
            {
                return item;
            }
        }

        // No match found
        // LogDebugError($"No Match Found in _clanArmoryItems available:({_clanArmoryItems.Count})");
        return null;
    }

    internal CrpgClanArmoryItem? FindArmoryItem(int userItemId)
    {
        return _clanArmoryItems.FirstOrDefault(x => x?.UserItem?.Id == userItemId);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<ServerSendArmoryActionResult>(HandleArmoryActionResult);
        registerer.Register<ServerSendClanArmoryItemUpdate>(HandleClanArmoryItemUpdate);
        registerer.Register<ServerSendClanArmoryCompleteUpdate>(HandleClanArmoryCompleteUpdate);
    }

    private void HandleArmoryActionResult(ServerSendArmoryActionResult message)
    {
        // Update local list as needed
        if (!message.Success)
        {
            InformationManager.DisplayMessage(new InformationMessage($"{message.ActionType} was unsuccessful."));
            InformationManager.DisplayMessage(new InformationMessage($"{message.ErrorMessage}", Colors.Red));
            return;
        }

        InformationManager.DisplayMessage(new InformationMessage($"{message.ActionType} success!!.", Colors.Yellow));
        // OnArmoryActionUpdated?.Invoke(message.ActionType, message.UserItemId);
    }

    private void HandleClanArmoryItemUpdate(ServerSendClanArmoryItemUpdate message)
    {
        LogDebug("--HandleClanArmoryItemUpdate started");
        int userItemId = message.ArmoryItem?.UserItem?.Id ?? -1;
        if (userItemId < 0)
        {
            LogDebugError($"HandleClanArmoryItemUpdate: userItemId < 0");
            return;
        }

        var existingItem = _clanArmoryItems.FirstOrDefault(x => x.UserItem?.Id == userItemId);

        // CrpgClanArmoryItem? clanArmoryItem = FindClanArmoryItemByUserItemId(userItemId);

        // Update _clanArmoryItems according to action
        switch (message.ActionType)
        {
            case ClanArmoryActionType.Add:
                // Add if it doesn't exist
                if (existingItem == null)
                {
                    if (message.ArmoryItem is not null)
                    {
                        _clanArmoryItems.Add(message.ArmoryItem);
                    }
                    else
                    {
                        LogDebugError($"Add error. message.ArmoryItem is null");
                    }
                }
                else if (existingItem.UserItem is not null)
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
                if (existingItem != null && existingItem.UserItem != null)
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
                    // Update fields individually to preserve references
                    // existingItem.UserItem = message.ArmoryItem.UserItem;
                    if (existingItem.UserItem is not null)
                    {
                        existingItem.UserItem.IsArmoryItem = true;
                    }

                    existingItem.BorrowedItem = null;
                    existingItem.UpdatedAt = message.ArmoryItem.UpdatedAt;

                }
                else
                {
                    LogDebugError("ExistingItem is null, or message.ArmoryItem is null");
                }

                break;

            default:
                break;
        }

        OnArmoryActionUpdated?.Invoke(message.ActionType, userItemId);

        // OnClanArmoryUpdated?.Invoke();
    }

    private void HandleClanArmoryCompleteUpdate(ServerSendClanArmoryCompleteUpdate message)
    {
        LogDebugError($"HandleClanArmoryCompleteUpdate: items({message.ArmoryItems.Count})");
        SetClanArmoryItems(message.ArmoryItems);
    }

    private void RemoveClanArmoryItem(int userItemId)
    {
        var itemToRemove = _clanArmoryItems
            .FirstOrDefault(e => e.UserItem?.Id == userItemId);

        if (itemToRemove != null)
        {
            _clanArmoryItems.Remove(itemToRemove);
        }
    }

    private readonly bool _debugOn = false;
    private void LogDebug(string message)
    {
        if (_debugOn)
        {
            LogDebug(message, Color.White);
        }
    }

    private void LogDebugError(string message)
    {
        LogDebug($"{GetType().Name} {message}", Colors.Red);
    }

    private void LogDebug(string message, Color color)
    {
        Debug.Print(message);
        InformationManager.DisplayMessage(new InformationMessage(message, color));
    }
}

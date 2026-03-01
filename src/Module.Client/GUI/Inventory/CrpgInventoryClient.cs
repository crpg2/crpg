using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

/// <summary>
/// Client-side behavior that receives inventory sync messages from the server.
/// Accumulates items between <see cref="SyncInventoryBegin"/> and <see cref="SyncInventoryEnd"/>,
/// using the sequence ID to discard stale streams.
/// </summary>
internal class CrpgInventoryClient : MissionNetwork
{
    private List<CrpgOwnedItem>? _pendingItems;
    private int _activeSequenceId = -1;

    /// <summary>Fired when a complete inventory sync stream has been received.</summary>
    public event Action<IList<CrpgOwnedItem>>? OnInventoryReceived;

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<SyncInventoryBegin>(HandleSyncInventoryBegin);
        registerer.Register<SyncInventoryItem>(HandleSyncInventoryItem);
        registerer.Register<SyncInventoryEnd>(HandleSyncInventoryEnd);
    }

    private void HandleSyncInventoryBegin(SyncInventoryBegin message)
    {
        _activeSequenceId = message.SequenceId;
        _pendingItems = [];
    }

    private void HandleSyncInventoryItem(SyncInventoryItem message)
    {
        if (message.SequenceId != _activeSequenceId || _pendingItems == null)
        {
            return;
        }

        _pendingItems.Add(new CrpgOwnedItem(message.Item, message.Rank, message.IsBroken));
    }

    private void HandleSyncInventoryEnd(SyncInventoryEnd message)
    {
        if (message.SequenceId != _activeSequenceId || _pendingItems == null)
        {
            return;
        }

        OnInventoryReceived?.Invoke(_pendingItems);
        _pendingItems = null;
        _activeSequenceId = -1;
    }
}

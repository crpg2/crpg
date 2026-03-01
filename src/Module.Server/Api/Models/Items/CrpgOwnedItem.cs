using TaleWorlds.Core;

namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// An item owned by a player, resolved to a Bannerlord <see cref="ItemObject"/>.
/// Used after the API/network boundary where string IDs have been resolved.
/// </summary>
internal class CrpgOwnedItem
{
    public ItemObject Item { get; }
    public int Rank { get; }
    public bool IsBroken { get; }

    public CrpgOwnedItem(ItemObject item, int rank, bool isBroken)
    {
        Item = item;
        Rank = rank;
        IsBroken = isBroken;
    }
}

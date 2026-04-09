using Crpg.Domain.Common;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Items;

/// <summary>
/// Item owned by a <see cref="User"/>.
/// </summary>
public class UserItem : AuditableEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public bool IsBroken { get; set; }
    public User? User { get; set; }
    public Item? Item { get; set; }

    /// <summary>
    /// Characters with that item equipped.
    /// </summary>
    public List<EquippedItem> EquippedItems { get; set; } = new();
    public ClanArmoryItem? ClanArmoryItem { get; set; }

    public PersonalItem? PersonalItem { get; set; }

    /// <summary>
    /// If set, this item is exclusive to the clan referenced by <see cref="ClanItem.ClanId"/>.
    /// Only members of that clan can see and equip it.
    /// </summary>
    public ClanItem? ClanItem { get; set; }

    public ClanArmoryBorrowedItem? ClanArmoryBorrowedItem { get; set; }
}

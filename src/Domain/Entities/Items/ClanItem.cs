using Crpg.Domain.Common;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Domain.Entities.Items;

/// <summary>
/// Marks a <see cref="UserItem"/> as exclusive to a specific <see cref="Clan"/>.
/// Only members of the clan can see and equip the item. The item is provisioned
/// to each member's inventory when assigned, and removed when they leave the clan.
/// </summary>
public class ClanItem : AuditableEntity
{
    public int UserItemId { get; set; }
    public int ClanId { get; set; }

    public UserItem? UserItem { get; set; }
    public Clan? Clan { get; set; }
}

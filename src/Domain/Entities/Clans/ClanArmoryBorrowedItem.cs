using Crpg.Domain.Common;
using Crpg.Domain.Entities.Items;

namespace Crpg.Domain.Entities.Clans;

public class ClanArmoryBorrowedItem : AuditableEntity
{
    public int BorrowerClanId { get; set; }
    public int BorrowerUserId { get; set; }
    public int UserItemId { get; set; }

    public ClanArmoryItem? ArmoryItem { get; set; }
    public UserItem? UserItem { get; set; }
    public ClanMember? Borrower { get; set; }
    public Clan? Clan { get; set; }
}

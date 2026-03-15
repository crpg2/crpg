using Crpg.Domain.Common;
using Crpg.Domain.Entities.Clans;
namespace Crpg.Domain.Entities.Items;

public class ClanArmoryItem : AuditableEntity
{
    public int LenderClanId { get; set; }
    public int LenderUserId { get; set; }
    public int UserItemId { get; set; }

    public UserItem? UserItem { get; set; }
    public ClanArmoryBorrowedItem? BorrowedItem { get; set; }
    public ClanMember? Lender { get; set; }
    public Clan? Clan { get; set; }
}

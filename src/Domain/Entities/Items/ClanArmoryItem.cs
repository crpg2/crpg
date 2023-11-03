﻿using Crpg.Domain.Common;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Domain.Entities.Items;
public class ClanArmoryItem : AuditableEntity
{
    public int ClanId { get; set; }
    public int UserId { get; set; }
    public int UserItemId { get; set; }

    public UserItem? UserItem { get; set; }
    public ClanArmoryBorrow? Borrow { get; set; }
    public ClanMember? ClanMember { get; set; }
    public Clan? Clan { get; set; }
}
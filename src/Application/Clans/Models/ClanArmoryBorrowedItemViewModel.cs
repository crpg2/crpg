using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models;

public class ClanArmoryBorrowedItemViewModel : IMapFrom<ClanArmoryBorrowedItem>
{
    public int BorrowerUserId { get; set; }
    public int UserItemId { get; set; }
}

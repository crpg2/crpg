using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Clans.Models;

public class ClanArmoryBorrowedItemUserViewModel : IMapFrom<ClanArmoryBorrowedItem>
{
    public UserItem UserItem { get; set; } = default!;
}

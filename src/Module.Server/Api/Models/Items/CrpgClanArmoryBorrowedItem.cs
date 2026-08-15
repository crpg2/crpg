namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy of Crpg.Application.Clans.Models.ClanArmoryBorrowedItemViewModel.
/// </summary>
internal class CrpgClanArmoryBorrowedItem
{
    public int BorrowerUserId { get; set; }
    public int UserItemId { get; set; }
    public DateTime UpdatedAt { get; set; }
}

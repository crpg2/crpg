namespace Crpg.Module.Api.Models.Items;

/// <summary>
/// Copy of Crpg.Application.Items.Models.GameUserItemExtendedViewModel.
/// </summary>
public class CrpgUserItemExtended
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Rank { get; set; } = default;
    public string ItemId { get; set; } = default!;
    public bool IsBroken { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsArmoryItem { get; set; }
    public bool IsPersonal { get; set; }
}

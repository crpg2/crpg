using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api.Models;

/// <summary>
/// Copy of Crpg.Application.Games.Models.UpdateGameUserResult.
/// </summary>
internal class UpdateCrpgUserResult
{
    public CrpgUser User { get; set; } = null!;
    public CrpgUserEffectiveReward EffectiveReward { get; set; } = null!;
    public IList<CrpgRepairedItem> RepairedItems { get; set; } = Array.Empty<CrpgRepairedItem>();
}

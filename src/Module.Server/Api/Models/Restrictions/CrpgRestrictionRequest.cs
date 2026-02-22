namespace Crpg.Module.Api.Models.Restrictions;

internal class CrpgRestrictionRequest
{
    public int RestrictedUserId { get; set; }
    public TimeSpan Duration { get; set; }
    public CrpgRestrictionType Type { get; set; }
    public string Reason { get; set; } = null!;
    public string PublicReason { get; set; } = null!;
    public int RestrictedByUserId { get; set; }
}

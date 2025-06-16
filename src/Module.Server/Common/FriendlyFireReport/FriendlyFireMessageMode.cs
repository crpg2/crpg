namespace Crpg.Module.Common.FriendlyFireReport;

public enum FriendlyFireMessageMode : byte
{
    Default = 0,
    TeamDamageReportForVictim,
    TeamDamageReportForAdmins,
    TeamDamageReportForAttacker,
    TeamDamageReportKick,
    TeamDamageReportError,
}

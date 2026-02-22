using Crpg.Module.Modes.TrainingGround;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.Common;

internal class CrpgTrainingGroundScoreboardData : IScoreboardData
{
    public MissionScoreboardComponent.ScoreboardHeader[] GetScoreboardHeaders()
    {
        GameNetwork.MyPeer.GetComponent<MissionRepresentativeBase>();
        return new MissionScoreboardComponent.ScoreboardHeader[]
        {
            new("ping", missionPeer => MathF.Round(missionPeer.GetNetworkPeer().AveragePingInMilliseconds).ToString(), _ => "BOT"),
            new("level", missionPeer => missionPeer.GetComponent<CrpgPeer>().User?.Character.Level.ToString() ?? string.Empty, _ => string.Empty),
            new("clan", missionPeer =>
                {
                    var crpgPeer = missionPeer.GetComponent<CrpgPeer>();
                    if (crpgPeer.Clan == null)
                    {
                        return string.Empty;
                    }

                    if (!crpgPeer.Clan.Name.Any(c => c >= '\u4e00' && c <= '\u9fa5') && crpgPeer.Clan.Name.Length <= 10)
                    {
                        return crpgPeer.Clan.Name;
                    }

                    return crpgPeer.Clan.Tag;
                },
                _ => string.Empty),
            new("name", missionPeer => missionPeer.DisplayedName, _ => new TextObject("{=hvQSOi79}Bot").ToString()),
            new("win", missionPeer => missionPeer.GetComponent<CrpgTrainingGroundMissionRepresentative>().NumberOfWins.ToString(), bot => bot.KillCount.ToString()),
            new("loss", missionPeer => missionPeer.GetComponent<CrpgTrainingGroundMissionRepresentative>().NumberOfLosses.ToString(), bot => bot.DeathCount.ToString()),
            new("rating", missionPeer => missionPeer.GetComponent<CrpgTrainingGroundMissionRepresentative>().Rating.ToString(), bot => bot.DeathCount.ToString()),
        };
    }
}

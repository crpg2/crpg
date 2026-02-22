using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.Module.Common;

internal class CrpgDuelScoreboardData : IScoreboardData
{
    public MissionScoreboardComponent.ScoreboardHeader[] GetScoreboardHeaders()
    {
        GameNetwork.MyPeer.GetComponent<MissionRepresentativeBase>();
        return new MissionScoreboardComponent.ScoreboardHeader[]
        {
            new("ping", missionPeer => TaleWorlds.Library.MathF.Round(missionPeer.GetNetworkPeer().AveragePingInMilliseconds).ToString(), _ => "BOT"),
            new("level", missionPeer => missionPeer.GetComponent<CrpgPeer>().User?.Character.Level.ToString() ?? string.Empty, _ => string.Empty),
            new("clan", missionPeer =>
                {
                    var crpgPeer = missionPeer.GetComponent<CrpgPeer>();
                    if (crpgPeer.Clan == null)
                    {
                        return string.Empty;
                    }

                    if (crpgPeer.Clan.Name.Length <= 10)
                    {
                        return crpgPeer.Clan.Name;
                    }
                    else
                    {
                        return crpgPeer.Clan.Tag;
                    }
                },
                _ => string.Empty),
            new("name", missionPeer => missionPeer.DisplayedName, _ => new TextObject("{=hvQSOi79}Bot").ToString()),
            new("winstreak", missionPeer => missionPeer.GetComponent<DuelMissionRepresentative>().NumberOfWins.ToString(), bot => string.Empty),
            new("bounty", missionPeer => missionPeer.GetComponent<DuelMissionRepresentative>().Bounty.ToString(), bot => string.Empty),
            new("score", missionPeer => missionPeer.GetComponent<DuelMissionRepresentative>().Score.ToString(), bot => string.Empty),
        };
    }
}

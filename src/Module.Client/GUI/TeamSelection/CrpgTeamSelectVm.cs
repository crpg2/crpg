using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.Module;
using Crpg.Module.GUI.HudExtension;
using JetBrains.Annotations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer.TeamSelection
{
    public class CrpgTeamSelectVM : ViewModel
    {
        private MissionRepresentativeBase? missionRep
        {
            get
            {
                NetworkCommunicator myPeer = GameNetwork.MyPeer;
                if (myPeer == null)
                {
                    return null;
                }

                VirtualPlayer virtualPlayer = myPeer.VirtualPlayer;
                if (virtualPlayer == null)
                {
                    return null;
                }

                return virtualPlayer.GetComponent<MissionRepresentativeBase>();
            }
        }

        public CrpgTeamSelectVM(Mission mission, Action<Team> onChangeTeamTo, Action onAutoAssign, Action onClose, IEnumerable<Team> teams, string gamemode)
        {
            _onClose = onClose;
            _onAutoAssign = onAutoAssign;
            _gamemodeStr = gamemode;

            _gameMode = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
            MissionScoreboardComponent missionBehavior = mission.GetMissionBehavior<MissionScoreboardComponent>();

            IsRoundCountdownAvailable = _gameMode.IsGameModeUsingRoundCountdown;

            Team spectatorTeam = teams.FirstOrDefault((Team t) => t.Side == BattleSideEnum.None);
            TeamSpectators = new CrpgTeamSelectInstanceVM(missionBehavior, spectatorTeam, null, null, onChangeTeamTo, false, new TextObject("{=pSheKLB4}Spectator", null).ToString());

            Team team1 = teams.FirstOrDefault((Team t) => t.Side == BattleSideEnum.Attacker);
            BasicCultureObject culture1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));

            var banners = Mission.Current.GetMissionBehavior<CrpgCustomTeamBannersAndNamesClient>();
            Banner attackerBanner = new(string.Empty);
            Banner defenderBanner = new(string.Empty);
            if (Mission.Current.Teams.Count > 0)
            {
                attackerBanner = new(Mission.Current.Teams.Attacker.Banner);
                defenderBanner = new(Mission.Current.Teams.Defender.Banner);
            }

            Team1 = new CrpgTeamSelectInstanceVM(
                missionBehavior,
                team1,
                culture1,
                banners?.AttackerBannerCode != null ? new(new Banner(banners?.AttackerBannerCode)) : new(attackerBanner),
                onChangeTeamTo,
                false,
                banners?.AttackerName ?? string.Empty);

            Team team2 = teams.FirstOrDefault((Team t) => t.Side == BattleSideEnum.Defender);
            BasicCultureObject culture2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
            Team2 = new CrpgTeamSelectInstanceVM(
                missionBehavior,
                team1,
                culture1,
                banners?.DefenderBannerCode != null ? new(new Banner(banners?.DefenderBannerCode)) : new(defenderBanner),
                onChangeTeamTo,
                false,
                banners?.DefenderName ?? string.Empty);

            if (GameNetwork.IsMyPeerReady)
            {
                _missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
                IsCancelDisabled = _missionPeer.Team == null;
            }

            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            AutoassignLbl = new TextObject("{=bON4Kn6B}Auto Assign", null).ToString();
            TeamSelectTitle = new TextObject("{=aVixswW5}Team Selection", null).ToString();
            GamemodeLbl = GameTexts.FindText("str_multiplayer_official_game_type_name", _gamemodeStr).ToString();
            Team1.RefreshValues();
            _team2.RefreshValues();
            _teamSpectators.RefreshValues();
        }

        public void Tick(float dt)
        {
            RemainingRoundTime = TimeSpan.FromSeconds((double)MathF.Ceiling(_gameMode.RemainingTime)).ToString("mm':'ss");
        }

        public void RefreshDisabledTeams(List<Team> disabledTeams)
        {
            if (disabledTeams == null)
            {
                CrpgTeamSelectInstanceVM teamSpectators = TeamSpectators;
                if (teamSpectators != null)
                {
                    teamSpectators.SetIsDisabled(false, false);
                }

                CrpgTeamSelectInstanceVM team = Team1;
                if (team != null)
                {
                    team.SetIsDisabled(false, false);
                }

                CrpgTeamSelectInstanceVM team2 = Team2;
                if (team2 == null)
                {
                    return;
                }

                team2.SetIsDisabled(false, false);
                return;
            }
            else
            {
                CrpgTeamSelectInstanceVM teamSpectators2 = TeamSpectators;
                if (teamSpectators2 != null)
                {
                    bool isCurrentTeam = false;
                    bool disabledForBalance;
                    if (disabledTeams == null)
                    {
                        disabledForBalance = false;
                    }
                    else
                    {
                        CrpgTeamSelectInstanceVM teamSpectators3 = TeamSpectators;
                        disabledForBalance = teamSpectators3 == null ? false : disabledTeams.Contains(teamSpectators3.Team);
                    }

                    teamSpectators2.SetIsDisabled(isCurrentTeam, disabledForBalance);
                }

                CrpgTeamSelectInstanceVM team3 = Team1;
                if (team3 != null)
                {
                    CrpgTeamSelectInstanceVM team4 = Team1;
                    Team? team5 = (team4 != null) ? team4.Team : null;
                    MissionPeer missionPeer = _missionPeer;
                    bool isCurrentTeam2 = team5 == ((missionPeer != null) ? missionPeer.Team : null);
                    bool disabledForBalance2;
                    if (disabledTeams == null)
                    {
                        disabledForBalance2 = false;
                    }
                    else
                    {
                        CrpgTeamSelectInstanceVM team6 = Team1;
                        disabledForBalance2 = team6 == null ? false : disabledTeams.Contains(team6.Team);
                    }

                    team3.SetIsDisabled(isCurrentTeam2, disabledForBalance2);
                }

                CrpgTeamSelectInstanceVM team7 = Team2;
                if (team7 == null)
                {
                    return;
                }

                CrpgTeamSelectInstanceVM team8 = Team2;
                Team? team9 = (team8 != null) ? team8.Team : null;
                MissionPeer missionPeer2 = _missionPeer;
                bool isCurrentTeam3 = team9 == ((missionPeer2 != null) ? missionPeer2.Team : null);
                bool disabledForBalance3;
                if (disabledTeams == null)
                {
                    disabledForBalance3 = false;
                }
                else
                {
                    CrpgTeamSelectInstanceVM team10 = Team2;
                    disabledForBalance3 = team10 == null ? false : disabledTeams.Contains(team10.Team);
                }

                team7.SetIsDisabled(isCurrentTeam3, disabledForBalance3);
                return;
            }
        }

        public void RefreshPlayerAndBotCount(int playersCountOne, int playersCountTwo, int botsCountOne, int botsCountTwo)
        {
            MBTextManager.SetTextVariable("PLAYER_COUNT", playersCountOne.ToString(), false);
            Team1.DisplayedSecondary = new TextObject("{=Etjqamlh}{PLAYER_COUNT} Players", null).ToString();
            MBTextManager.SetTextVariable("BOT_COUNT", botsCountOne.ToString(), false);
            Team1.DisplayedSecondarySub = new TextObject("{=eCOJSSUH}({BOT_COUNT} Bots)", null).ToString();
            MBTextManager.SetTextVariable("PLAYER_COUNT", playersCountTwo.ToString(), false);
            Team2.DisplayedSecondary = new TextObject("{=Etjqamlh}{PLAYER_COUNT} Players", null).ToString();
            MBTextManager.SetTextVariable("BOT_COUNT", botsCountTwo.ToString(), false);
            Team2.DisplayedSecondarySub = new TextObject("{=eCOJSSUH}({BOT_COUNT} Bots)", null).ToString();
        }

        public void RefreshFriendsPerTeam(IEnumerable<MissionPeer> friendsTeamOne, IEnumerable<MissionPeer> friendsTeamTwo)
        {
            Team1.RefreshFriends(friendsTeamOne);
            Team2.RefreshFriends(friendsTeamTwo);
        }

        [UsedImplicitly]
        public void ExecuteCancel()
        {
            _onClose();
        }

        [UsedImplicitly]
        public void ExecuteAutoAssign()
        {
            _onAutoAssign();
        }

        [DataSourceProperty]
        public CrpgTeamSelectInstanceVM Team1
        {
            get
            {
                return _team1;
            }
            set
            {
                if (value != _team1)
                {
                    _team1 = value;
                    OnPropertyChangedWithValue(value, "Team1");
                }
            }
        }

        [DataSourceProperty]
        public CrpgTeamSelectInstanceVM Team2
        {
            get
            {
                return _team2;
            }
            set
            {
                if (value != _team2)
                {
                    _team2 = value;
                    OnPropertyChangedWithValue(value, "Team2");
                }
            }
        }

        [DataSourceProperty]
        public CrpgTeamSelectInstanceVM TeamSpectators
        {
            get
            {
                return _teamSpectators;
            }
            set
            {
                if (value != _teamSpectators)
                {
                    _teamSpectators = value;
                    OnPropertyChangedWithValue(value, "TeamSpectators");
                }
            }
        }

        [DataSourceProperty]
        public string TeamSelectTitle
        {
            get
            {
                return _teamSelectTitle;
            }
            set
            {
                _teamSelectTitle = value;
                OnPropertyChangedWithValue(value, "TeamSelectTitle");
            }
        }

        [DataSourceProperty]
        public bool IsRoundCountdownAvailable
        {
            get
            {
                return _isRoundCountdownAvailable;
            }
            set
            {
                if (value != _isRoundCountdownAvailable)
                {
                    _isRoundCountdownAvailable = value;
                    OnPropertyChangedWithValue(value, "IsRoundCountdownAvailable");
                }
            }
        }

        [DataSourceProperty]
        public string RemainingRoundTime
        {
            get
            {
                return _remainingRoundTime;
            }
            set
            {
                if (value != _remainingRoundTime)
                {
                    _remainingRoundTime = value;
                    OnPropertyChangedWithValue(value, "RemainingRoundTime");
                }
            }
        }

        [DataSourceProperty]
        public string GamemodeLbl
        {
            get
            {
                return _gamemodeLbl;
            }
            set
            {
                _gamemodeLbl = value;
                OnPropertyChangedWithValue(value, "GamemodeLbl");
            }
        }

        [DataSourceProperty]
        public string AutoassignLbl
        {
            get
            {
                return _autoassignLbl;
            }
            set
            {
                _autoassignLbl = value;
                OnPropertyChangedWithValue(value, "AutoassignLbl");
            }
        }

        [DataSourceProperty]
        public bool IsCancelDisabled
        {
            get
            {
                return _isCancelDisabled;
            }
            set
            {
                _isCancelDisabled = value;
                OnPropertyChangedWithValue(value, "IsCancelDisabled");
            }
        }

        private readonly Action _onClose;

        private readonly Action _onAutoAssign;

        private readonly MissionMultiplayerGameModeBaseClient _gameMode = default!;

        private readonly MissionPeer _missionPeer = default!;

        private readonly string _gamemodeStr;

        private string _teamSelectTitle = default!;

        private bool _isRoundCountdownAvailable;

        private string _remainingRoundTime = default!;

        private string _gamemodeLbl = default!;

        private string _autoassignLbl = default!;

        private bool _isCancelDisabled;

        private CrpgTeamSelectInstanceVM _team1 = default!;

        private CrpgTeamSelectInstanceVM _team2 = default!;

        private CrpgTeamSelectInstanceVM _teamSpectators = default!;
        private string _toggleMuteText = default!;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.Module.GUI.HudExtension;
using Crpg.Module.Modes.Conquest;
using JetBrains.Annotations;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

namespace TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer.TeamSelection
{
    public class CrpgTeamSelectInstanceVM : ViewModel
    {
        public CrpgTeamSelectInstanceVM(MissionScoreboardComponent missionScoreboardComponent, Team team, BasicCultureObject? culture, BannerImageIdentifierVM? banner, Action<Team> onSelect, bool useSecondary, string teamName)
        {
            Team = team;
            UseSecondary = useSecondary;
            _onSelect = onSelect;
            _culture = culture;
            Mission mission = Mission.Current;
            IsSiege = mission != null && mission.HasMissionBehavior<CrpgConquestClient>();
            if (Team != null && Team.Side != BattleSideEnum.None)
            {
                _missionScoreboardComponent = missionScoreboardComponent;
                _missionScoreboardComponent.OnRoundPropertiesChanged += UpdateTeamScores;
                _missionScoreboardSide = _missionScoreboardComponent.Sides.FirstOrDefault((MissionScoreboardComponent.MissionScoreboardSide s) => s != null && s.Side == Team.Side);
                IsAttacker = Team.Side == BattleSideEnum.Attacker;
                UpdateTeamScores();
            }

            CultureId = (culture == null) ? string.Empty : culture.StringId;
            if (team == null)
            {
                IsDisabled = true;
            }

            if (banner == null)
            {
                Banner = banner;
            }
            else
            {
                Banner = banner;
            }

            _friends = new List<MPPlayerVM>();
            FriendAvatars = new MBBindingList<MPPlayerVM>();
            RefreshValues();
            DisplayedPrimary = teamName;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public override void OnFinalize()
        {
            if (_missionScoreboardComponent != null)
            {
                _missionScoreboardComponent.OnRoundPropertiesChanged -= UpdateTeamScores;
            }

            _missionScoreboardComponent = null;
            _missionScoreboardSide = null;
            base.OnFinalize();
        }

        private void UpdateTeamScores()
        {
            if (_missionScoreboardSide != null)
            {
                Score = _missionScoreboardSide.SideScore;
            }
        }

        public void RefreshFriends(IEnumerable<MissionPeer> friends)
        {
            List<MissionPeer> list = friends.ToList<MissionPeer>();
            List<MPPlayerVM> list2 = new();
            foreach (MPPlayerVM mpplayerVM in _friends)
            {
                if (!list.Contains(mpplayerVM.Peer))
                {
                    list2.Add(mpplayerVM);
                }
            }

            foreach (MPPlayerVM item in list2)
            {
                _friends.Remove(item);
            }

            List<MissionPeer> list3 = _friends.Select((MPPlayerVM x) => x.Peer).ToList<MissionPeer>();
            foreach (MissionPeer missionPeer in list)
            {
                if (!list3.Contains(missionPeer))
                {
                    _friends.Add(new MPPlayerVM(missionPeer));
                }
            }

            FriendAvatars.Clear();
            MBStringBuilder mbstringBuilder = default!;
            mbstringBuilder.Initialize(16, "RefreshFriends");
            for (int i = 0; i < _friends.Count; i++)
            {
                if (i < 6)
                {
                    FriendAvatars.Add(_friends[i]);
                }
                else
                {
                    mbstringBuilder.AppendLine<string>(_friends[i].Peer.DisplayedName);
                }
            }

            int num = _friends.Count - 6;
            if (num > 0)
            {
                HasExtraFriends = true;
                TextObject textObject = new("{=hbwp3g3k}+{FRIEND_COUNT} {newline} {?PLURAL}friends{?}friend{\\?}", null);
                textObject.SetTextVariable("FRIEND_COUNT", num);
                textObject.SetTextVariable("PLURAL", (num == 1) ? 0 : 1);
                FriendsExtraText = textObject.ToString();
                FriendsExtraHint = new HintViewModel(textObject, null);
                return;
            }

            mbstringBuilder.Release();
            HasExtraFriends = false;
            FriendsExtraText = string.Empty;
        }

        public void SetIsDisabled(bool isCurrentTeam, bool disabledForBalance)
        {
            IsDisabled = isCurrentTeam || disabledForBalance;
            if (isCurrentTeam)
            {
                LockText = new TextObject("{=SoQcsslF}CURRENT TEAM", null).ToString();
                return;
            }

            if (disabledForBalance)
            {
                LockText = new TextObject("{=qe46yXVJ}LOCKED FOR BALANCE", null).ToString();
            }
        }

        [UsedImplicitly]
        public void ExecuteSelectTeam()
        {
            if (_onSelect != null)
            {
                _onSelect(Team);
            }
        }

        [DataSourceProperty]
        public string CultureId
        {
            get
            {
                return _cultureId;
            }
            set
            {
                if (_cultureId != value)
                {
                    _cultureId = value;
                    OnPropertyChangedWithValue(value, "CultureId");
                }
            }
        }

        [DataSourceProperty]
        public int Score
        {
            get
            {
                return _score;
            }
            set
            {
                if (value != _score)
                {
                    _score = value;
                    OnPropertyChangedWithValue(value, "Score");
                }
            }
        }

        [DataSourceProperty]
        public bool IsDisabled
        {
            get
            {
                return _isDisabled;
            }
            set
            {
                if (_isDisabled != value)
                {
                    _isDisabled = value;
                    OnPropertyChangedWithValue(value, "IsDisabled");
                }
            }
        }

        [DataSourceProperty]
        public bool UseSecondary
        {
            get
            {
                return _useSecondary;
            }
            set
            {
                if (_useSecondary != value)
                {
                    _useSecondary = value;
                    OnPropertyChangedWithValue(value, "UseSecondary");
                }
            }
        }

        [DataSourceProperty]
        public bool IsAttacker
        {
            get
            {
                return _isAttacker;
            }
            set
            {
                if (_isAttacker != value)
                {
                    _isAttacker = value;
                    OnPropertyChangedWithValue(value, "IsAttacker");
                }
            }
        }

        [DataSourceProperty]
        public bool IsSiege
        {
            get
            {
                return _isSiege;
            }
            set
            {
                if (_isSiege != value)
                {
                    _isSiege = value;
                    OnPropertyChangedWithValue(value, "IsSiege");
                }
            }
        }

        [DataSourceProperty]
        public string DisplayedPrimary
        {
            get
            {
                return _displayedPrimary;
            }
            set
            {
                _displayedPrimary = value;
                OnPropertyChangedWithValue(value, "DisplayedPrimary");
            }
        }

        [DataSourceProperty]
        public string DisplayedSecondary
        {
            get
            {
                return _displayedSecondary;
            }
            set
            {
                _displayedSecondary = value;
                OnPropertyChangedWithValue(value, "DisplayedSecondary");
            }
        }

        [DataSourceProperty]
        public string DisplayedSecondarySub
        {
            get
            {
                return _displayedSecondarySub;
            }
            set
            {
                _displayedSecondarySub = value;
                OnPropertyChangedWithValue(value, "DisplayedSecondarySub");
            }
        }

        [DataSourceProperty]
        public string LockText
        {
            get
            {
                return _lockText;
            }
            set
            {
                _lockText = value;
                OnPropertyChangedWithValue(value, "LockText");
            }
        }

        [DataSourceProperty]
        public BannerImageIdentifierVM? Banner
        {
            get
            {
                return _banner;
            }
            set
            {
                if (value != _banner && (value == null || _banner == null || _banner.Id != value.Id))
                {
                    _banner = value;
                    OnPropertyChangedWithValue(value, "Banner");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<MPPlayerVM> FriendAvatars
        {
            get
            {
                return _friendAvatars;
            }
            set
            {
                if (_friendAvatars != value)
                {
                    _friendAvatars = value;
                    OnPropertyChangedWithValue(value, "FriendAvatars");
                }
            }
        }

        [DataSourceProperty]
        public bool HasExtraFriends
        {
            get
            {
                return _hasExtraFriends;
            }
            set
            {
                if (_hasExtraFriends != value)
                {
                    _hasExtraFriends = value;
                    OnPropertyChangedWithValue(value, "HasExtraFriends");
                }
            }
        }

        [DataSourceProperty]
        public string FriendsExtraText
        {
            get
            {
                return _friendsExtraText;
            }
            set
            {
                if (_friendsExtraText != value)
                {
                    _friendsExtraText = value;
                    OnPropertyChangedWithValue(value, "FriendsExtraText");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel FriendsExtraHint
        {
            get
            {
                return _friendsExtraHint;
            }
            set
            {
                if (_friendsExtraHint != value)
                {
                    _friendsExtraHint = value;
                    OnPropertyChangedWithValue(value, "FriendsExtraHint");
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM? AllyBanner
        {
            get
            {
                return _allyBanner;
            }
            set
            {
                if (value == _allyBanner)
                {
                    return;
                }

                _allyBanner = value;
                OnPropertyChangedWithValue(value);
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM? EnemyBanner
        {
            get
            {
                return _enemyBanner;
            }
            set
            {
                if (value == _enemyBanner)
                {
                    return;
                }

                _enemyBanner = value;
                OnPropertyChangedWithValue(value);
            }
        }

        private const int MaxFriendAvatarCount = 6;

        public readonly Team Team = default!;

        public readonly Action<Team> _onSelect;

        private readonly List<MPPlayerVM> _friends;

        private MissionScoreboardComponent? _missionScoreboardComponent = default!;

        private MissionScoreboardComponent.MissionScoreboardSide? _missionScoreboardSide = default!;

        private readonly BasicCultureObject? _culture = default!;

        private bool _isDisabled;

        private string _displayedPrimary = default!;

        private string _displayedSecondary = default!;

        private string _displayedSecondarySub = default!;

        private string _lockText = default!;

        private string _cultureId = default!;

        private int _score;

        private BannerImageIdentifierVM? _banner = default!;

        private MBBindingList<MPPlayerVM> _friendAvatars = default!;

        private bool _hasExtraFriends;

        private bool _useSecondary;

        private bool _isAttacker;

        private bool _isSiege;

        private string _friendsExtraText = default!;

        private HintViewModel _friendsExtraHint = default!;
        private ImageIdentifierVM? _allyBanner;
        private ImageIdentifierVM? _enemyBanner;
    }
}

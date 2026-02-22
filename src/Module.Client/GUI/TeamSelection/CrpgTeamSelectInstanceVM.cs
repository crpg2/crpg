using Crpg.Module.Modes.Conquest;
using JetBrains.Annotations;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

namespace Crpg.Module.GUI.TeamSelection;

public class CrpgTeamSelectInstanceVm : ViewModel
{
    private readonly Action<Team>? _onSelect;
    private readonly List<MPPlayerVM> _friends;
    private MissionScoreboardComponent? _missionScoreboardComponent;
    private MissionScoreboardComponent.MissionScoreboardSide? _missionScoreboardSide;
    private bool _isDisabled;
    private string _displayedPrimary = null!;
    private string _displayedSecondary = null!;
    private string _displayedSecondarySub = null!;
    private string _lockText = null!;
    private string _cultureId = null!;
    private int _score;
    private BannerImageIdentifierVM? _banner;
    private MBBindingList<MPPlayerVM> _friendAvatars = null!;
    private bool _hasExtraFriends;
    private bool _useSecondary;
    private bool _isAttacker;
    private bool _isSiege;
    private string _friendsExtraText = null!;
    private HintViewModel _friendsExtraHint = null!;
    private ImageIdentifierVM? _allyBanner;
    private ImageIdentifierVM? _enemyBanner;

    public CrpgTeamSelectInstanceVm(MissionScoreboardComponent missionScoreboardComponent, Team? team,
        BasicCultureObject? culture, BannerImageIdentifierVM? banner, Action<Team> onSelect, bool useSecondary,
        string teamName)
    {
        Team = team;
        UseSecondary = useSecondary;
        _onSelect = onSelect;
        Mission mission = Mission.Current;
        IsSiege = mission != null && mission.HasMissionBehavior<CrpgConquestClient>();
        if (Team != null && Team.Side != BattleSideEnum.None)
        {
            _missionScoreboardComponent = missionScoreboardComponent;
            _missionScoreboardComponent.OnRoundPropertiesChanged += UpdateTeamScores;
            _missionScoreboardSide =
                _missionScoreboardComponent.Sides.FirstOrDefault(s => s != null && s.Side == Team.Side);
            IsAttacker = Team.Side == BattleSideEnum.Attacker;
            UpdateTeamScores();
        }

        CultureId = culture == null ? string.Empty : culture.StringId;
        if (team == null)
        {
            IsDisabled = true;
        }

        Banner = banner;

        _friends = new List<MPPlayerVM>();
        FriendAvatars = new MBBindingList<MPPlayerVM>();
        RefreshValues();
        DisplayedPrimary = teamName;
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
            OnPropertyChangedWithValue(value);
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
            OnPropertyChangedWithValue(value);
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
            OnPropertyChangedWithValue(value);
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
            OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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

    public Team? Team { get; set; }

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

    public void RefreshFriends(IEnumerable<MissionPeer> friends)
    {
        List<MissionPeer> list = friends.ToList();
        List<MPPlayerVM> list2 = new();
        foreach (MPPlayerVM mpplayerVm in _friends)
        {
            if (!list.Contains(mpplayerVm.Peer))
            {
                list2.Add(mpplayerVm);
            }
        }

        foreach (MPPlayerVM item in list2)
        {
            _friends.Remove(item);
        }

        List<MissionPeer> list3 = _friends.Select(x => x.Peer).ToList();
        foreach (MissionPeer missionPeer in list)
        {
            if (!list3.Contains(missionPeer))
            {
                _friends.Add(new MPPlayerVM(missionPeer));
            }
        }

        FriendAvatars.Clear();
        MBStringBuilder mbstringBuilder = default!;
        mbstringBuilder.Initialize();
        for (int i = 0; i < _friends.Count; i++)
        {
            if (i < 6)
            {
                FriendAvatars.Add(_friends[i]);
            }
            else
            {
                mbstringBuilder.AppendLine(_friends[i].Peer.DisplayedName);
            }
        }

        int num = _friends.Count - 6;
        if (num > 0)
        {
            HasExtraFriends = true;
            TextObject textObject = new("{=hbwp3g3k}+{FRIEND_COUNT} {newline} {?PLURAL}friends{?}friend{\\?}");
            textObject.SetTextVariable("FRIEND_COUNT", num);
            textObject.SetTextVariable("PLURAL", num == 1 ? 0 : 1);
            FriendsExtraText = textObject.ToString();
            FriendsExtraHint = new HintViewModel(textObject);
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
            LockText = new TextObject("{=SoQcsslF}CURRENT TEAM").ToString();
            return;
        }

        if (disabledForBalance)
        {
            LockText = new TextObject("{=qe46yXVJ}LOCKED FOR BALANCE").ToString();
        }
    }

    [UsedImplicitly]
    public void ExecuteSelectTeam()
    {
        if (_onSelect != null && Team != null)
        {
            _onSelect(Team);
        }
    }

    private void UpdateTeamScores()
    {
        if (_missionScoreboardSide != null)
        {
            Score = _missionScoreboardSide.SideScore;
        }
    }
}

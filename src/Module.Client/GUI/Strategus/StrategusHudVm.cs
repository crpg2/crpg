using Crpg.Module.GUI.Hud;
using Crpg.Module.Modes.Strategus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.HUDExtensions;

namespace Crpg.Module.GUI.Strategus;

internal class StrategusHudVm : ViewModel
{
    private readonly CrpgStrategusClient _client;
    private BannerHudVm _allyBannerVm;
    private Team? _allyTeam;
    private Team? _enemyTeam;
    private BannerHudVm _enemyBannerVm;
    private TimerHudVm _timerVm;
    private int _allyTicketCount;
    private int _enemyTicketCount;
    private bool _isGameStarted;

    public StrategusHudVm(Mission mission)
    {
        MissionPeer.OnTeamChanged += OnTeamChanged;
        _allyBannerVm = new BannerHudVm(mission, allyBanner: true);
        _enemyBannerVm = new BannerHudVm(mission, allyBanner: false);
        _timerVm = new TimerHudVm(mission);
        _client = mission.GetMissionBehavior<CrpgStrategusClient>();
        RefreshValues();
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
    }

    [DataSourceProperty]
    public BannerHudVm AllyBanner
    {
        get => _allyBannerVm;
        set
        {
            if (value != _allyBannerVm)
            {
                _allyBannerVm = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public BannerHudVm EnemyBanner
    {
        get => _enemyBannerVm;
        set
        {
            if (value != _enemyBannerVm)
            {
                _enemyBannerVm = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int AllyTicketCount
    {
        get => _allyTicketCount;
        set
        {
            if (value != _allyTicketCount)
            {
                _allyTicketCount = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int EnemyTicketCount
    {
        get => _enemyTicketCount;
        set
        {
            if (value != _enemyTicketCount)
            {
                _enemyTicketCount = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsGameStarted
    {
        get => _isGameStarted;
        set
        {
            if (value == _isGameStarted)
            {
                return;
            }

            _isGameStarted = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public TimerHudVm Timer
    {
        get => _timerVm;
        set
        {
            if (value != _timerVm)
            {
                _timerVm = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    public void Tick(float dt)
    {
        _timerVm.Tick(dt);
        IsGameStarted = !_client.IsInWarmup;
    }

    public void UpdateTicketCount()
    {
        if (_allyTeam == null)
        {
            return;
        }

        AllyTicketCount = _allyTeam.Side == BattleSideEnum.Attacker ? _client.AttackerTicketCount : _client.DefenderTicketCount;
        EnemyTicketCount = _allyTeam.Side == BattleSideEnum.Attacker ? _client.DefenderTicketCount : _client.AttackerTicketCount;
    }

    private void OnTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
    {
            OnTeamChanged();
    }

    private void OnTeamChanged()
    {
        _allyTeam = GameNetwork.MyPeer.GetComponent<MissionPeer>().Team;
        if (_allyTeam == null)
        {
            return;
        }

        if (!GameNetwork.IsMyPeerReady)
        {
            return;
        }

        _enemyTeam = Mission.Current.Teams.FirstOrDefault(t => t.IsEnemyOf(_allyTeam));
        if (_allyTeam.Side == BattleSideEnum.None)
        {
            _allyTeam = Mission.Current.AttackerTeam;
        }
    }
}

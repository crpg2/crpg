using Crpg.Module.GUI.Hud;
using Crpg.Module.Modes.Strategus;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Strategus;

internal class StrategusHudVm : ViewModel
{
    private readonly CrpgStrategusClient _client;
    private BannerHudVm _allyBannerVm;
    private BannerHudVm _enemyBannerVm;
    private TimerHudVm _timerVm;
    private int _defenderTicketCount;
    private int _attackerTicketCount;
    private bool _isGameStarted;

    public StrategusHudVm(Mission mission)
    {
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
    public int AttackerTicketCount
    {
        get => _attackerTicketCount;
        set
        {
            if (value != _attackerTicketCount)
            {
                _attackerTicketCount = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int DefenderTicketCount
    {
        get => _defenderTicketCount;
        set
        {
            if (value != _defenderTicketCount)
            {
                _defenderTicketCount = value;
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
        AttackerTicketCount = _client.AttackerTicketCount;
        DefenderTicketCount = _client.DefenderTicketCount;
    }
}

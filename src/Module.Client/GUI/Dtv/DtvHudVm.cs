using Crpg.Module.GUI.Hud;
using Crpg.Module.Modes.Dtv;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.GUI.Dtv;

internal class DtvHudVm : ViewModel
{
    private readonly CrpgDtvClient _client;
    private BannerHudVm _allyBannerVm;
    private BannerHudVm _enemyBannerVm;
    private TimerHudVm _timerVm;
    private string _waveLabel = string.Empty;
    private string _roundLabel = string.Empty;
    private int _defenderMemberCount;
    private int _attackerMemberCount;
    private int _currentWave;
    private int _currentRound;
    private bool _isGameStarted;
    private int _vipHealth = 0;
    private int _lastVipHealth = 0;
    private bool _isVipHealthBarVisible = false;
    private TaleWorlds.GauntletUI.Brush _vipHealthBrush;

    public DtvHudVm(Mission mission)
    {
        _allyBannerVm = new BannerHudVm(mission, allyBanner: true);
        _enemyBannerVm = new BannerHudVm(mission, allyBanner: false);
        _timerVm = new TimerHudVm(mission);
        _client = mission.GetMissionBehavior<CrpgDtvClient>();
        _vipHealthBrush = UIResourceManager.BrushFactory.GetBrush("CrpgHUD.Dtv.HealthBarBrush.Healthy");
        RefreshValues();
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        WaveLabel = new TextObject("{=8KlA48nt}Wave").ToString();
        RoundLabel = new TextObject("{=1QzB15at}Round").ToString();
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
    public int AttackerMemberCount
    {
        get => _attackerMemberCount;
        set
        {
            if (value != _attackerMemberCount)
            {
                _attackerMemberCount = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int DefenderMemberCount
    {
        get => _defenderMemberCount;
        set
        {
            if (value != _defenderMemberCount)
            {
                _defenderMemberCount = value;
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
    public string RoundLabel
    {
        get => _roundLabel;
        set
        {
            if (value != _roundLabel)
            {
                _roundLabel = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string WaveLabel
    {
        get => _waveLabel;
        set
        {
            if (value != _waveLabel)
            {
                _waveLabel = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int CurrentRound
    {
        get => _currentRound;
        set
        {
            if (value != _currentRound)
            {
                _currentRound = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int CurrentWave
    {
        get => _currentWave;
        set
        {
            if (value != _currentWave)
            {
                _currentWave = value;
                OnPropertyChangedWithValue(value);
            }
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

    [DataSourceProperty]
    public bool IsVipHealthBarVisible
    {
        get => _isVipHealthBarVisible;
        set
        {
            if (value != _isVipHealthBarVisible)
            {
                _isVipHealthBarVisible = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int VipHealth
    {
        get => _vipHealth;
        set
        {
            if (_vipHealth != value)
            {
                _vipHealth = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public TaleWorlds.GauntletUI.Brush VipHealthBrush
    {
        get => _vipHealthBrush;
        set
        {
            if (_vipHealthBrush != value)
            {
                _vipHealthBrush = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    public void Tick(float dt)
    {
        _timerVm.Tick(dt);
        IsGameStarted = !_client.IsInWarmup;
        if (IsGameStarted && Mission.Current.Agents.Count > 0)
        {
            AttackerMemberCount = Mission.Current.AttackerTeam.ActiveAgents.Count;
            DefenderMemberCount = Mission.Current.DefenderTeam.ActiveAgents.Count;

            UpdateVipAgentHealthBar();
        }
    }

    public void UpdateProgress()
    {
        CurrentWave = _client.CurrentWave;
        CurrentRound = _client.CurrentRound;
    }

    public void UpdateVipAgentHealthBar()
    {
        var agent = _client?.VipAgent;

        if (agent == null || !agent.IsActive())
        {
            if (_lastVipHealth != 0)
            {
                VipHealth = 0;
                _lastVipHealth = 0;
                IsVipHealthBarVisible = false;
            }

            return;
        }

        float healthRatio = MathF.Clamp(agent.Health / agent.HealthLimit, 0f, 1f);
        int newVipHealth = (int)MathF.Round(healthRatio * 100);

        if (newVipHealth == _lastVipHealth)
        {
            return;
        }

        VipHealth = newVipHealth;
        _lastVipHealth = newVipHealth;

        VipHealthBrush = UIResourceManager.BrushFactory.GetBrush(
            newVipHealth > 75 ? "CrpgHUD.Dtv.HealthBarBrush.Healthy" :
            newVipHealth > 25 ? "CrpgHUD.Dtv.HealthBarBrush.Injured" :
                                "CrpgHUD.Dtv.HealthBarBrush.Critical");

        IsVipHealthBarVisible = true;
    }
}

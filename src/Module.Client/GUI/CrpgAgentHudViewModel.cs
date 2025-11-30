using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI;

internal class CrpgAgentHudViewModel : ViewModel
{
    private readonly BreakableWeaponsBehaviorClient _breakClient;
    private readonly CrpgExperienceTable _experienceTable;
    private readonly NetworkCommunicator _myPeer;
    private int _experience;
    private float _levelProgression;
    private int _rewardMultiplier;
    private string _rewardMultiplierStr = string.Empty;
    private bool _showExperienceBar;
    private bool _showWeaponBar;
    private int _weaponHealth;
    private int _weaponHealthMax;
    private string _lastRoll = string.Empty;
    private string _lastBlow = string.Empty;
    private bool _showRoll;

    public CrpgAgentHudViewModel(CrpgExperienceTable experienceTable)
    {
        _breakClient = Mission.Current.GetMissionBehavior<BreakableWeaponsBehaviorClient>();
        _experienceTable = experienceTable;
        _myPeer = GameNetwork.MyPeer;
        _weaponHealth = 100;
        _weaponHealthMax = 100;
    }

    /// <summary>
    /// A number between 0.0 and 1.0 for the level progression of the player.
    /// </summary>
    [DataSourceProperty]
    public float LevelProgression
    {
        get => _levelProgression;
        private set
        {
            _levelProgression = value;
            OnPropertyChangedWithValue(value); // Notify that the property changed.
        }
    }

    [DataSourceProperty]
    public string RewardMultiplier
    {
        get => _rewardMultiplierStr;
        private set
        {
            _rewardMultiplierStr = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool ShowExperienceBar
    {
        get => _showExperienceBar;
        private set
        {
            _showExperienceBar = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool ShowWeaponBar
    {
        get => _showWeaponBar;
        private set
        {
            _showWeaponBar = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public int WeaponHealth
    {
        get => _weaponHealth;
        private set
        {
            _weaponHealth = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public int WeaponHealthMax
    {
        get => _weaponHealthMax;
        private set
        {
            _weaponHealthMax = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string LastRoll
    {
        get => _lastRoll;
        private set
        {
            _lastRoll = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string LastBlow
    {
        get => _lastBlow;
        private set
        {
            _lastBlow = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool ShowRoll
    {
        get => _showRoll;
        private set
        {
            _showRoll = value;
            OnPropertyChangedWithValue(value);
        }
    }

    public void Tick(float deltaTime)
    {
        // Hide the experience bar if the user is dead.
        ShowExperienceBar = Mission.Current?.MainAgent != null;

        var crpgPeer = _myPeer.GetComponent<CrpgPeer>();

        if (crpgPeer == null)
        {
            return;
        }

        var user = crpgPeer.User;
        if (user == null)
        {
            return;
        }

        if (_experience != user.Character.Experience)
        {
            float levelProgression = ComputeLevelProgression(user);
            // Clamp if for some reason the level and experience are not synchronized.
            LevelProgression = MathF.Clamp(levelProgression, 0.0f, 1.0f);
            _experience = user.Character.Experience;
        }

        if (_rewardMultiplier != crpgPeer.RewardMultiplier)
        {
            RewardMultiplier = 'x' + crpgPeer.RewardMultiplier.ToString();
            _rewardMultiplier = crpgPeer.RewardMultiplier;
        }

        var missionPeer = _myPeer.GetComponent<MissionPeer>();
        if (BreakableWeaponsBehaviorServer.
            BreakAbleItemsHitPoints.
            TryGetValue(
            missionPeer?.ControlledAgent?.WieldedWeapon.Item?.StringId ?? string.Empty,
            out short healthMax))
        {
            WeaponHealthMax = healthMax;
            WeaponHealth = missionPeer!.ControlledAgent!.WieldedWeapon.HitPoints;
            ShowWeaponBar = true;
            if (WeaponHealth == 1)
            {
                ShowRoll = true && ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.ReportDamage) > 0;
                LastRoll = _breakClient.LastRoll.ToString();
                LastBlow = _breakClient.LastBlow.ToString();
            }
            else
            {
                _showRoll = false;
            }
        }
        else
        {
            ShowWeaponBar = false;
        }
    }

    private float ComputeLevelProgression(CrpgUser user)
    {
        int experienceForCurrentLevel = _experienceTable.GetExperienceForLevel(user.Character.Level);
        int experienceForNextLevel = _experienceTable.GetExperienceForLevel(user.Character.Level + 1);

        if (experienceForCurrentLevel == experienceForNextLevel) // Max level
        {
            return 1f;
        }

        return (float)(user.Character.Experience - experienceForCurrentLevel) / (experienceForNextLevel - experienceForCurrentLevel);
    }
}

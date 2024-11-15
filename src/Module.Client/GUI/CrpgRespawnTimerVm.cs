using Crpg.Module.Common;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI;
internal class CrpgRespawnTimerVm : ViewModel
{
    private readonly CrpgRespawnTimerClient _client;

    private string _respawnText = string.Empty;
    private float _timeToRespawn;
    private bool _isVisible;

    public CrpgRespawnTimerVm(Mission mission)
    {
        _client = mission.GetMissionBehavior<CrpgRespawnTimerClient>();
        RefreshValues();
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        TextObject respawnTextObj = new("{=l81jAS1c}You will respawn in {TIME} {?PLURAL}seconds!{?}second!{\\?}", null);
        respawnTextObj.SetTextVariable("PLURAL", ((int)_timeToRespawn == 1) ? 0 : 1);
        respawnTextObj.SetTextVariable("TIME", (int)_timeToRespawn);
        RespawnText = respawnTextObj.ToString();
        IsVisible = _timeToRespawn > 0;
    }

    [DataSourceProperty]
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (value != _isVisible)
            {
                _isVisible = value;
                OnPropertyChangedWithValue(value, "IsVisible");
            }
        }
    }

    [DataSourceProperty]
    public string RespawnText
    {
        get => _respawnText;
        set
        {
            if (value != _respawnText)
            {
                _respawnText = value;
                OnPropertyChangedWithValue(value, "RespawnText");
            }
        }
    }

    public void Tick(float dt)
    {
        _timeToRespawn -= dt;
        RefreshValues();
    }

    public void Update()
    {
        _timeToRespawn = _client.RespawnTimer;
    }
}

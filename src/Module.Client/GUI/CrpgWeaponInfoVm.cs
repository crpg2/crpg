using Crpg.Module.GUI.Hud;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI;

internal class CrpgWeaponInfoVm : ViewModel
{
    private int _weaponUsageIndex = -1;
    private bool _showWeaponUsageInfo = false;
    private string _weaponUsageName = "Unset";

    [DataSourceProperty]
    public MBBindingList<HudTextLineVm> Lines { get; } = new MBBindingList<HudTextLineVm>();

    public CrpgWeaponInfoVm(Mission mission)
    {
        WeaponUsageIndex = 0;
        WeaponUsageName = "Unset";
        ShowWeaponUsageInfo = true;
    }

    public void UpdateLines(IEnumerable<string> newLines)
    {
        Lines.Clear();
        foreach (string line in newLines)
        {
            Lines.Add(new HudTextLineVm(line));
        }
    }

    public void AddLine(string text, string style = "Default", float lifetime = 10f)
    {
        Lines.Add(new HudTextLineVm(text, style, lifetime));
    }

    public void OnMissionScreenTick(float dt)
    {
    }

    public sealed override void RefreshValues()
    {
        base.RefreshValues();
    }

    [DataSourceProperty]
    public int WeaponUsageIndex
    {
        get => _weaponUsageIndex;
        set
        {
            if (value != _weaponUsageIndex)
            {
                _weaponUsageIndex = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string WeaponUsageName
    {
        get => _weaponUsageName;
        set
        {
            if (value != _weaponUsageName)
            {
                _weaponUsageName = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool ShowWeaponUsageInfo
    {
        get => _showWeaponUsageInfo;
        set
        {
            if (value != _showWeaponUsageInfo)
            {
                _showWeaponUsageInfo = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }
}

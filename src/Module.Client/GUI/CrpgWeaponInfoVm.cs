using Crpg.Module.GUI.Hud;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI;

internal class CrpgWeaponInfoVm : ViewModel
{
    private int _weaponUsageIndex = -1;
    private bool _showWeaponUsageIndex = false;

    public CrpgWeaponInfoVm(Mission mission)
    {
        WeaponUsageIndex = 0;
        ShowWeaponUsageIndex = true;
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
    public bool ShowWeaponUsageIndex
    {
        get => _showWeaponUsageIndex;
        set
        {
            if (value != _showWeaponUsageIndex)
            {
                _showWeaponUsageIndex = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }
}

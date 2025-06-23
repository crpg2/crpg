using System.Drawing.Text;
using Crpg.Module.Common;
using Crpg.Module.Common.Commander;
using Crpg.Module.Helpers;
using Crpg.Module.Modes.Dtv;
using Crpg.Module.Modes.Duel;
using Crpg.Module.Modes.Siege;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker.Targets;
using TaleWorlds.MountAndBlade.Objects;

namespace Crpg.Module.GUI;

internal class CrpgWeaponInfoVm : ViewModel
{
    private int _weaponUsageIndex = -1;
    private bool _showWeaponUsageInfo = false;
    private string _weaponUsageName = "Unset";

    public CrpgWeaponInfoVm(Mission mission)
    {
        WeaponUsageIndex = 0;
        WeaponUsageName = "Unset";
        ShowWeaponUsageInfo = true;
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

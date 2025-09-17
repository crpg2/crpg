using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class ArmorAmountVM : ViewModel
{
    private int _armorAmount;
    [DataSourceProperty]
    public int ArmorAmount
    {
        get => _armorAmount;
        set
        {
            if (_armorAmount != value)
            {
                _armorAmount = value;
                OnPropertyChanged(nameof(ArmorAmount));
            }
        }
    }

    public ArmorAmountVM(int armorAmount = 0)
    {
        ArmorAmount = armorAmount;
    }
}

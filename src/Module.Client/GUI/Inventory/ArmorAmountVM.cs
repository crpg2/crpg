using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class ArmorAmountVM : ViewModel
{
    private int _armorAmount;

    [DataSourceProperty]
    public int ArmorAmount { get => _armorAmount; set => SetField(ref _armorAmount, value, nameof(ArmorAmount)); }

    public ArmorAmountVM(int armorAmount = 0)
    {
        ArmorAmount = armorAmount;
    }
}

using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class ArmorAmountVM : ViewModel
{
    [DataSourceProperty]
    public int ArmorAmount { get; set => SetField(ref field, value, nameof(ArmorAmount)); }
    private readonly string _tooltipText;

    public ArmorAmountVM(int armorAmount = 0, string tooltipText = "")
    {
        ArmorAmount = armorAmount;
        _tooltipText = tooltipText;
    }

    private void ExecuteBeginHint()
    {
        if (!string.IsNullOrEmpty(_tooltipText))
        {
            MBInformationManager.ShowHint(_tooltipText);
        }
    }

    private void ExecuteEndHint() => MBInformationManager.HideInformations();
}

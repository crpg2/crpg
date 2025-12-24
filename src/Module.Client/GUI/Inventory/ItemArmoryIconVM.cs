using Crpg.Module.Api.Models;
using Crpg.Module.Common;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class ItemArmoryIconVM : ViewModel
{
    private readonly CrpgClanArmoryClient? _clanArmory;

    [DataSourceProperty]
    public int ItemArmoryStatus
    {
        get;
        set
        {
            if (SetField(ref field, value, nameof(ItemArmoryStatus)))
            {
                var converted = (CrpgGameArmoryItemStatus)value;
                YoursAvailable = converted == CrpgGameArmoryItemStatus.YoursAvailable;
                YoursBorrowed = converted == CrpgGameArmoryItemStatus.YoursBorrowed;
                NotYoursAvailable = converted == CrpgGameArmoryItemStatus.NotYoursAvailable;
                NotYoursBorrowed = converted == CrpgGameArmoryItemStatus.NotYoursBorrowed;
                BorrowedByYou = converted == CrpgGameArmoryItemStatus.BorrowedByYou;
            }
        }
    }

    [DataSourceProperty]
    public bool YoursAvailable { get; set => SetField(ref field, value, nameof(YoursAvailable)); }
    [DataSourceProperty]
    public bool YoursBorrowed { get; set => SetField(ref field, value, nameof(YoursBorrowed)); }
    [DataSourceProperty]
    public bool NotYoursAvailable { get; set => SetField(ref field, value, nameof(NotYoursAvailable)); }
    [DataSourceProperty]
    public bool NotYoursBorrowed { get; set => SetField(ref field, value, nameof(NotYoursBorrowed)); }
    [DataSourceProperty]
    public bool BorrowedByYou { get; set => SetField(ref field, value, nameof(BorrowedByYou)); }

    public ItemArmoryIconVM(int initialStatus = -1)
    {
        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();
        ItemArmoryStatus = initialStatus;
    }

    internal void UpdateItemArmoryIconFromItem(int userItemId)
    {
        ItemArmoryStatus = (_clanArmory is not null && _clanArmory.GetCrpgUserItemArmoryStatus(userItemId, out var status))
            ? (int)status
            : -1;
    }
}

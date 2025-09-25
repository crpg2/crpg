using Crpg.Module.Common;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class ItemArmoryIconVM : ViewModel
{
    internal CrpgCharacterLoadoutBehaviorClient? UserLoadoutBehavior { get; set; }
    private int _itemArmoryStatus = -1;
    private bool _yoursAvailable;
    private bool _yoursBorrowed;
    private bool _notYoursAvailible;
    private bool _notYoursBorrowed;
    private bool _borrowedByYou;

    [DataSourceProperty]
    public int ItemArmoryStatus
    {
        get => _itemArmoryStatus;
        set
        {
            if (SetField(ref _itemArmoryStatus, value, nameof(ItemArmoryStatus)))
            {
                SetArmoryIconsVisible(value);
            }
        }
    }

    [DataSourceProperty]
    public bool YoursAvailable { get => _yoursAvailable; set => SetField(ref _yoursAvailable, value, nameof(YoursAvailable)); }
    [DataSourceProperty]
    public bool YoursBorrowed { get => _yoursBorrowed; set => SetField(ref _yoursBorrowed, value, nameof(YoursBorrowed)); }
    [DataSourceProperty]
    public bool NotYoursAvailible { get => _notYoursAvailible; set => SetField(ref _notYoursAvailible, value, nameof(NotYoursAvailible)); }
    [DataSourceProperty]
    public bool NotYoursBorrowed { get => _notYoursBorrowed; set => SetField(ref _notYoursBorrowed, value, nameof(NotYoursBorrowed)); }
    [DataSourceProperty]
    public bool BorrowedByYou { get => _borrowedByYou; set => SetField(ref _borrowedByYou, value, nameof(BorrowedByYou)); }

    public ItemArmoryIconVM(int initialStatus = -1)
    {
        UserLoadoutBehavior = Mission.Current!.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        ItemArmoryStatus = initialStatus;
        SetArmoryIconsVisible(initialStatus);
    }

    internal void UpdateItemArmoyIconFromItem(int userItemId)
    {
        if (UserLoadoutBehavior is not null && UserLoadoutBehavior.GetCrpgUserItemArmoryStatus(userItemId, out var itemArmoryStatus))
        {
            SetArmoryIconsVisible((int)itemArmoryStatus);
        }
        else
        {
            SetArmoryIconsVisible(-1);
        }
    }

    private void SetArmoryIconsVisible(int status)
    {
        var converted = (CrpgCharacterLoadoutBehaviorClient.CrpgGameArmoryItemStatus)status;
        // InformationManager.DisplayMessage(new InformationMessage($"ItemArmoryIconVM set to {converted}"));
        // status 0 no image
        YoursAvailable = false;
        YoursBorrowed = false;
        NotYoursAvailible = false;
        NotYoursBorrowed = false;
        BorrowedByYou = false;

        switch (converted)
        {
            case CrpgCharacterLoadoutBehaviorClient.CrpgGameArmoryItemStatus.YoursAvailable:
                YoursAvailable = true;
                break;
            case CrpgCharacterLoadoutBehaviorClient.CrpgGameArmoryItemStatus.YoursBorrowed:
                YoursBorrowed = true;
                break;
            case CrpgCharacterLoadoutBehaviorClient.CrpgGameArmoryItemStatus.NotYoursAvailible:
                NotYoursAvailible = true;
                break;
            case CrpgCharacterLoadoutBehaviorClient.CrpgGameArmoryItemStatus.NotYoursBorrowed:
                NotYoursBorrowed = true;
                break;
            case CrpgCharacterLoadoutBehaviorClient.CrpgGameArmoryItemStatus.BorrowedByYou:
                BorrowedByYou = true;
                break;
        }
    }
}

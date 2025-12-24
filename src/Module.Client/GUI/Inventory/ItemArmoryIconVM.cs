using Crpg.Module.Api.Models;
using Crpg.Module.Common;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class ItemArmoryIconVM : ViewModel
{
    private readonly CrpgClanArmoryClient? _clanArmory;
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
        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();
        if (_clanArmory is null)
        {
            _clanArmory = new CrpgClanArmoryClient();
            Mission.Current?.AddMissionBehavior(_clanArmory);
        }

        ItemArmoryStatus = initialStatus;
        SetArmoryIconsVisible(initialStatus);
    }

    internal void UpdateItemArmoryIconFromItem(int userItemId)
    {
        if (_clanArmory is not null && _clanArmory.GetCrpgUserItemArmoryStatus(userItemId, out var itemArmoryStatus))
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
        var converted = (CrpgGameArmoryItemStatus)status;
        // InformationManager.DisplayMessage(new InformationMessage($"ItemArmoryIconVM set to {converted}"));
        // status -1 no image
        YoursAvailable = false;
        YoursBorrowed = false;
        NotYoursAvailible = false;
        NotYoursBorrowed = false;
        BorrowedByYou = false;

        switch (converted)
        {
            case CrpgGameArmoryItemStatus.YoursAvailable:
                YoursAvailable = true;
                break;
            case CrpgGameArmoryItemStatus.YoursBorrowed:
                YoursBorrowed = true;
                break;
            case CrpgGameArmoryItemStatus.NotYoursAvailible:
                NotYoursAvailible = true;
                break;
            case CrpgGameArmoryItemStatus.NotYoursBorrowed:
                NotYoursBorrowed = true;
                break;
            case CrpgGameArmoryItemStatus.BorrowedByYou:
                BorrowedByYou = true;
                break;
        }
    }
}

using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.GUI.Inventory;

public class EquipmentSlotVM : ViewModel
{
    private string? _cachedItemId;
    private CrpgUserItemExtended? _userItemEx;
    private ItemImageIdentifierVM? _imageIdentifier;
    private CrpgItemSlot _crpgItemSlotIndex;

    internal event Action<EquipmentSlotVM, ViewModel>? OnItemDropped;
    internal event Action<EquipmentSlotVM>? OnSlotAlternateClicked;
    internal event Action<EquipmentSlotVM>? OnSlotClicked;
    internal event Action<EquipmentSlotVM>? OnHoverBegin;
    internal event Action<EquipmentSlotVM>? OnHoverEnd;
    internal event Action<ItemObject>? OnItemDragBegin;
    internal event Action<ItemObject>? OnItemDragEnd;

    internal CrpgUserItemExtended? UserItemEx
    {
        get => _userItemEx;
        set
        {
            if (_userItemEx != value)
            {
                _userItemEx = value;
                RefreshDerivedState();
            }
        }
    }

    public EquipmentSlotVM(CrpgItemSlot crpgSlot)
    {
        _userItemEx = null;
        _crpgItemSlotIndex = crpgSlot;
        DefaultSprite = GetDefaultSpriteForSlot(_crpgItemSlotIndex);
        _imageIdentifier = new ItemImageIdentifierVM(null);
        IsButtonEnabled = true;
        ItemRankIcon = new ItemRankIconVM();
        ItemArmoryIcon = new ItemArmoryIconVM();
    }

    private void RefreshDerivedState()
    {
        UserItemId = UserItemEx?.Id ?? -1;

        string? newItemId = UserItemEx?.ItemId;

        // ONLY cache expensive stuff
        if (_cachedItemId != newItemId)
        {
            _cachedItemId = newItemId;

            ItemObj = newItemId != null
                ? MBObjectManager.Instance.GetObject<ItemObject>(newItemId)
                : null;

            ImageIdentifier = ItemObj != null
                ? new ItemImageIdentifierVM(ItemObj)
                : new ItemImageIdentifierVM(null);
        }

        // ALWAYS update these (they can change without itemId changing)
        IsTeamInventoryItem = CrpgTeamInventoryClient.IsTeamInventoryItem(UserItemId);

        ItemRank = IsTeamInventoryItem
            ? CrpgTeamInventoryClient.GetRankFromItemId(newItemId ?? string.Empty)
            : UserItemEx?.Rank ?? 0;

        IsArmoryItem = UserItemEx?.IsArmoryItem ?? false;

        ItemRankIcon.ItemRank = ItemRank;
        ItemArmoryIcon?.UpdateItemArmoryIconFromItem(UserItemId);
    }

    // ===== Event handlers for UI actions =====
    private void ExecuteTryEquipItem(ViewModel draggedItem, int index)
    {
        OnItemDropped?.Invoke(this, draggedItem);
    }

    private void ExecuteAlternateClick()
    {
        OnSlotAlternateClicked?.Invoke(this);
        MBInformationManager.HideInformations();
    }

    private void ExecuteClick()
    {
        OnSlotClicked?.Invoke(this);
    }

    private void ExecuteHoverBegin()
    {
        OnHoverBegin?.Invoke(this);
        if (ItemObj?.Name != null)
        {
            MBInformationManager.ShowHint(ItemObj.Name.ToString());
        }
        else
        {
            MBInformationManager.ShowHint(_crpgItemSlotIndex.ToString() ?? "");
        }
    }

    private void ExecuteHoverEnd()
    {
        OnHoverEnd?.Invoke(this);
        MBInformationManager.HideInformations();
    }

    private void ExecuteDragBegin()
    {
        IsDragging = true;
        if (ItemObj != null)
        {
            OnItemDragBegin?.Invoke(ItemObj);
        }
    }

    private void ExecuteDragEnd()
    {
        IsDragging = false;
        if (ItemObj != null)
        {
            OnItemDragEnd?.Invoke(ItemObj);
        }
    }

    [DataSourceProperty]
    public bool IsArmoryItem { get; set => SetField(ref field, value, nameof(IsArmoryItem)); }
    [DataSourceProperty]
    public bool IsTeamInventoryItem { get; set => SetField(ref field, value, nameof(IsTeamInventoryItem)); }
    [DataSourceProperty]
    public bool ShouldShowDefaultIcon => (_imageIdentifier == null || !_imageIdentifier.IsValid) || IsDragging;
    [DataSourceProperty]
    public bool CanAcceptDrag => ItemObj != null;
    [DataSourceProperty]
    public int UserItemId { get; set => SetField(ref field, value, nameof(UserItemId)); }
    [DataSourceProperty]
    public string DefaultSprite { get; set => SetField(ref field, value, nameof(DefaultSprite)); }
    [DataSourceProperty]
    public bool IsButtonEnabled { get; set => SetField(ref field, value, nameof(IsButtonEnabled)); }
    [DataSourceProperty]
    public int ItemRank { get; set => SetField(ref field, value, nameof(ItemRank)); } = 0;
    [DataSourceProperty]
    public ItemRankIconVM ItemRankIcon { get; set => SetField(ref field, value, nameof(ItemRankIcon)); }
    [DataSourceProperty]
    public ItemArmoryIconVM? ItemArmoryIcon { get; set => SetField(ref field, value, nameof(ItemArmoryIcon)); }

    [DataSourceProperty]
    public ItemImageIdentifierVM? ImageIdentifier
    {
        get => _imageIdentifier;
        set
        {
            if (_imageIdentifier != value)
            {
                _imageIdentifier = value;
                OnPropertyChanged(nameof(ImageIdentifier));
                OnPropertyChanged(nameof(ShouldShowDefaultIcon));
            }
        }
    }

    [DataSourceProperty]
    public ItemObject? ItemObj
    {
        get => field;
        set
        {
            if (SetField(ref field, value, nameof(ItemObj)))
            {
                // Notify dependent property explicitly
                OnPropertyChanged(nameof(CanAcceptDrag));
            }
        }
    }

    [DataSourceProperty]
    public bool IsDragging
    {
        get => field;
        set
        {
            if (SetField(ref field, value, nameof(IsDragging)))
            {
                OnPropertyChanged(nameof(ShouldShowDefaultIcon));
            }
        }
    }

    [DataSourceProperty]
    public CrpgItemSlot CrpgItemSlotIndex
    {
        get => _crpgItemSlotIndex;
        set
        {
            if (SetField(ref _crpgItemSlotIndex, value, nameof(CrpgItemSlotIndex)))
            {
                DefaultSprite = GetDefaultSpriteForSlot(_crpgItemSlotIndex);
            }
        }
    }

    private static readonly Dictionary<CrpgItemSlot, EquipmentIndex> CrpgToEquipIndex = new()
    {
        [CrpgItemSlot.Head] = EquipmentIndex.Head,
        [CrpgItemSlot.Shoulder] = EquipmentIndex.Cape,
        [CrpgItemSlot.Body] = EquipmentIndex.Body,
        [CrpgItemSlot.Hand] = EquipmentIndex.Gloves,
        [CrpgItemSlot.Leg] = EquipmentIndex.Leg,
        [CrpgItemSlot.MountHarness] = EquipmentIndex.HorseHarness,
        [CrpgItemSlot.Mount] = EquipmentIndex.Horse,
        [CrpgItemSlot.Weapon0] = EquipmentIndex.Weapon0,
        [CrpgItemSlot.Weapon1] = EquipmentIndex.Weapon1,
        [CrpgItemSlot.Weapon2] = EquipmentIndex.Weapon2,
        [CrpgItemSlot.Weapon3] = EquipmentIndex.Weapon3,
        [CrpgItemSlot.WeaponExtra] = EquipmentIndex.ExtraWeaponSlot,
    };

    internal static EquipmentIndex ConvertToEquipmentIndex(CrpgItemSlot slot)
        => CrpgToEquipIndex.TryGetValue(slot, out var result) ? result : EquipmentIndex.None;

    private string GetDefaultSpriteForSlot(CrpgItemSlot slot) => slot switch
    {
        CrpgItemSlot.Head => "ui_crpg_icon_white_headarmor",
        CrpgItemSlot.Shoulder => "ui_crpg_icon_white_cape",
        CrpgItemSlot.Body => "ui_crpg_icon_white_chestarmor",
        CrpgItemSlot.Hand => "ui_crpg_icon_white_handarmor",
        CrpgItemSlot.Leg => "ui_crpg_icon_white_legarmor",
        CrpgItemSlot.Weapon0 => "ui_crpg_icon_white_weaponslot",
        CrpgItemSlot.Weapon1 => "ui_crpg_icon_white_weaponslot",
        CrpgItemSlot.Weapon2 => "ui_crpg_icon_white_weaponslot",
        CrpgItemSlot.Weapon3 => "ui_crpg_icon_white_weaponslot",
        CrpgItemSlot.WeaponExtra => "ui_crpg_icon_white_extraweapon",
        CrpgItemSlot.Mount => "ui_crpg_icon_white_mount",
        CrpgItemSlot.MountHarness => "ui_crpg_icon_white_mountharness",
        _ => "ui_crpg_icon_white_weaponslot",
    };
}

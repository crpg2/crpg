using System;
using System.Collections.Generic;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class EquipmentSlotVM : ViewModel
{
    private ImageIdentifierVM? _imageIdentifier;
    private bool _isDragging;
    private string _defaultSprite;
    private CrpgItemSlot _crpgItemSlotIndex;
    private ItemObject? _itemObj;
    private int _userItemId;
    private bool _isButtonEnabled;

    public event Action<EquipmentSlotVM, ViewModel>? OnItemDropped;
    public event Action<EquipmentSlotVM>? OnSlotAlternateClicked;
    public event Action<ItemObject>? OnItemDragBegin;
    public event Action<ItemObject>? OnItemDragEnd;

    public EquipmentSlotVM(CrpgItemSlot crpgSlot)
    {
        _crpgItemSlotIndex = crpgSlot;
        _defaultSprite = GetDefaultSpriteForSlot(_crpgItemSlotIndex);
        _imageIdentifier = new ImageIdentifierVM(ImageIdentifierType.Item);
        _isButtonEnabled = true;
    }

    public void ExecuteAlternateClick()
    {
        OnSlotAlternateClicked?.Invoke(this);
    }

    public void ExecuteDragBegin()
    {
        IsDragging = true;
        if (ItemObj != null)
        {
            OnItemDragBegin?.Invoke(ItemObj);
        }
    }

    public void ExecuteDragEnd()
    {
        IsDragging = false;
        if (ItemObj != null)
        {
            OnItemDragEnd?.Invoke(ItemObj);
        }
    }

    public void ExecuteTryEquipItem(ViewModel draggedItem, int index)
    {
        OnItemDropped?.Invoke(this, draggedItem);
    }

    public void SetItem(ImageIdentifierVM? newIdentifier, ItemObject? itemObj = null, int? userItemId = null)
    {
        ImageIdentifier = newIdentifier;
        ItemObj = itemObj;
        UserItemId = userItemId ?? 0;

        OnPropertyChanged(nameof(ItemObj));
        OnPropertyChanged(nameof(CanAcceptDrag));
    }

    public void ClearItem()
    {
        SetItem(new ImageIdentifierVM(ImageIdentifierType.Item));
    }

    [DataSourceProperty]
    public ImageIdentifierVM? ImageIdentifier
    {
        get => _imageIdentifier;
        set
        {
            if (_imageIdentifier != value)
            {
                _imageIdentifier = value;
                OnPropertyChanged(nameof(ImageIdentifier));
                OnPropertyChanged(nameof(ShouldShowDefaultIcon));
                OnPropertyChanged(nameof(CanAcceptDrag));
            }
        }
    }

    [DataSourceProperty]
    public bool ShouldShowDefaultIcon => (_imageIdentifier == null || !_imageIdentifier.IsValid) || _isDragging;

    [DataSourceProperty]
    public bool CanAcceptDrag => ItemObj != null;

    [DataSourceProperty]
    public int UserItemId
    {
        get => _userItemId;
        set
        {
            if (_userItemId != value)
            {
                _userItemId = value;
                OnPropertyChanged(nameof(UserItemId));
            }
        }
    }

    [DataSourceProperty]
    public string DefaultSprite
    {
        get => _defaultSprite;
        set
        {
            if (_defaultSprite != value)
            {
                _defaultSprite = value;
                OnPropertyChanged(nameof(DefaultSprite));
            }
        }
    }

    [DataSourceProperty]
    public CrpgItemSlot CrpgItemSlotIndex
    {
        get => _crpgItemSlotIndex;
        set
        {
            if (_crpgItemSlotIndex != value)
            {
                _crpgItemSlotIndex = value;
                DefaultSprite = GetDefaultSpriteForSlot(_crpgItemSlotIndex);
                OnPropertyChanged(nameof(CrpgItemSlotIndex));
            }
        }
    }

    [DataSourceProperty]
    public ItemObject? ItemObj
    {
        get => _itemObj;
        private set
        {
            if (_itemObj != value)
            {
                _itemObj = value;
                OnPropertyChanged(nameof(ItemObj));
            }
        }
    }

    [DataSourceProperty]
    public bool IsButtonEnabled
    {
        get => _isButtonEnabled;
        set
        {
            if (_isButtonEnabled != value)
            {
                _isButtonEnabled = value;
                OnPropertyChanged(nameof(IsButtonEnabled));
            }
        }
    }

    public bool IsDragging
    {
        get => _isDragging;
        set
        {
            if (_isDragging != value)
            {
                _isDragging = value;
                OnPropertyChanged(nameof(IsDragging));
                OnPropertyChanged(nameof(ShouldShowDefaultIcon));
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

    public static EquipmentIndex ConvertToEquipmentIndex(CrpgItemSlot slot)
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

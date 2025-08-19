using System;
using System.Collections.Generic;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class EquipmentSlotVM : ViewModel
{
    private readonly Action<EquipmentSlotVM>? _alternateClick;
    private ImageIdentifierVM? _imageIdentifier;
    private bool _isDragging;
    private string _defaultSprite;
    private CrpgItemSlot _crpgItemSlotIndex;
    private ItemObject? _itemObj;
    private int _userItemId;

    public event Action<EquipmentSlotVM>? OnSlotUpdated;

    public EquipmentSlotVM(ImageIdentifierVM imageIdentifier, CrpgItemSlot crpgSlot, Action<EquipmentSlotVM>? handleAlternateClick = null)
    {
        _imageIdentifier = imageIdentifier;
        _crpgItemSlotIndex = crpgSlot;
        _defaultSprite = GetDefaultSpriteForSlot(_crpgItemSlotIndex);
        _alternateClick = handleAlternateClick;

        _itemObj = (_imageIdentifier != null && _imageIdentifier.IsValid && _imageIdentifier.ImageTypeCode == (int)ImageIdentifierType.Item)
            ? Game.Current.ObjectManager.GetObject<ItemObject>(_imageIdentifier.Id)
            : null;

        RefreshValues();
    }

    public static EquipmentIndex ConvertToEquipmentIndex(CrpgItemSlot slot)
        => CrpgToEquipIndex.TryGetValue(slot, out var result) ? result : EquipmentIndex.None;

    public void ExecuteAlternateClick()
    {
        _alternateClick?.Invoke(this);

        if (ItemObj == null || UserItemId == 0)
        {
            InformationManager.DisplayMessage(new InformationMessage($"ExecuteAlternateClick() ItemObj or UserItemId is null"));
            return;
        }

        // Send unequip request; do NOT call SetItem here
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestEquipCharacterItem { Slot = CrpgItemSlotIndex, UserItemId = -1 });
        GameNetwork.EndModuleEventAsClient();
    }

    public void ExecuteDragBegin() => IsDragging = true;
    public void ExecuteDragEnd() => IsDragging = false;

    public void ExecuteTryEquipItem(ViewModel draggedItemVM, int index)
    {
        int userItemId = 0;

        if (draggedItemVM is InventorySlotVM inventorySlot)
            userItemId = inventorySlot.UserItemId;
        else if (draggedItemVM is EquipmentSlotVM equipmentSlot)
        {
            if (equipmentSlot.CrpgItemSlotIndex == CrpgItemSlotIndex || equipmentSlot.ItemObj == null)
                return;
            userItemId = equipmentSlot.UserItemId;
        }
        else
            return;

        // Send equip request; do NOT call SetItem here
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestEquipCharacterItem { Slot = CrpgItemSlotIndex, UserItemId = userItemId });
        GameNetwork.EndModuleEventAsClient();
    }

    // Called only from server-confirmed OnSlotUpdated
    public void SetItem(ImageIdentifierVM? newIdentifier, int? userItemId = null)
    {
        ImageIdentifier = newIdentifier;

        _itemObj = (newIdentifier != null && newIdentifier.IsValid)
            ? Game.Current.ObjectManager.GetObject<ItemObject>(newIdentifier.Id)
            : null;

        UserItemId = userItemId ?? 0;

        OnPropertyChanged(nameof(ItemObj));
        OnPropertyChanged(nameof(ImageIdentifier));
        OnPropertyChanged(nameof(UserItemId));

        //OnSlotUpdated?.Invoke(this);
    }

    public void ClearItem() => SetItem(new ImageIdentifierVM(ImageIdentifierType.Item));

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

    private string GetDefaultSpriteForSlot(CrpgItemSlot slot)
        => slot switch
        {
            CrpgItemSlot.Head => "General\\EquipmentIcons\\equipment_type_head_armor",
            CrpgItemSlot.Shoulder => "General\\EquipmentIcons\\equipment_type_cape",
            CrpgItemSlot.Body => "General\\EquipmentIcons\\equipment_type_body_armor",
            CrpgItemSlot.Hand => "General\\EquipmentIcons\\equipment_type_hand_armor",
            CrpgItemSlot.Leg => "General\\EquipmentIcons\\equipment_type_leg_armor",
            CrpgItemSlot.Weapon0 => "General\\EquipmentIcons\\equipment_type_default",
            CrpgItemSlot.Weapon1 => "General\\EquipmentIcons\\equipment_type_default",
            CrpgItemSlot.Weapon2 => "General\\EquipmentIcons\\equipment_type_default",
            CrpgItemSlot.Weapon3 => "General\\EquipmentIcons\\equipment_type_default",
            CrpgItemSlot.WeaponExtra => "General\\EquipmentIcons\\equipment_type_banner",
            CrpgItemSlot.Mount => "General\\EquipmentIcons\\equipment_type_mount",
            CrpgItemSlot.MountHarness => "General\\EquipmentIcons\\equipment_type_default",
            _ => "General\\EquipmentIcons\\equipment_type_default",
        };

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
    public ItemObject? ItemObj
    {
        get => _itemObj;
        set
        {
            if (_itemObj != value)
            {
                _itemObj = value;
                OnPropertyChanged(nameof(ItemObj));
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
                OnPropertyChanged(nameof(CrpgItemSlotIndex));
                DefaultSprite = GetDefaultSpriteForSlot(_crpgItemSlotIndex);
            }
        }
    }

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

    [DataSourceProperty] public bool ShouldShowDefaultIcon => (_imageIdentifier == null || !_imageIdentifier.IsValid) || _isDragging;

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

    [DataSourceProperty] public bool CanAcceptDrag => _imageIdentifier != null && _imageIdentifier.IsValid;
}
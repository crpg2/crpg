using System;
using Crpg.Module.Api.Models.Items;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class EquipmentSlotVM : ViewModel
{
    private readonly Action<EquipmentSlotVM>? _alternateClick;
    private ImageIdentifierVM? _imageIdentifier;
    private bool _showDefaultIcon;
    private bool _isDragging;
    private string _defaultSprite;
    private EquipmentIndex _equipmentSlot;
    private ItemObject? _itemObj;

    public EquipmentSlotVM(ImageIdentifierVM imageIdentifier, EquipmentIndex equipmentSlot, Action<EquipmentSlotVM>? handleAlternateClick = null)
    {
        _imageIdentifier = imageIdentifier;
        _equipmentSlot = equipmentSlot;
        _defaultSprite = GetDefaultSpriteForSlot(equipmentSlot);
        _alternateClick = handleAlternateClick;
        _showDefaultIcon = _imageIdentifier == null || !_imageIdentifier.IsValid;
        ItemObj = null;
        if (_imageIdentifier != null && _imageIdentifier.IsValid && _imageIdentifier.ImageTypeCode == (int)ImageIdentifierType.Item)
        {
            ItemObj = Game.Current.ObjectManager.GetObject<ItemObject>(_imageIdentifier.Id);
        }

        RefreshValues();
    }

    private string GetDefaultSpriteForSlot(EquipmentIndex slot)
    {
        return slot switch
        {
            EquipmentIndex.Head => "General\\EquipmentIcons\\equipment_type_head_armor",
            EquipmentIndex.Cape => "General\\EquipmentIcons\\equipment_type_cape",
            EquipmentIndex.Body => "General\\EquipmentIcons\\equipment_type_body_armor",
            EquipmentIndex.Gloves => "General\\EquipmentIcons\\equipment_type_hand_armor",
            EquipmentIndex.Leg => "General\\EquipmentIcons\\equipment_type_leg_armor",
            EquipmentIndex.Weapon0 => "General\\EquipmentIcons\\equipment_type_default",
            EquipmentIndex.Weapon1 => "General\\EquipmentIcons\\equipment_type_default",
            EquipmentIndex.Weapon2 => "General\\EquipmentIcons\\equipment_type_default",
            EquipmentIndex.Weapon3 => "General\\EquipmentIcons\\equipment_type_default",
            EquipmentIndex.ExtraWeaponSlot => "General\\EquipmentIcons\\equipment_type_banner",
            EquipmentIndex.Horse => "General\\EquipmentIcons\\equipment_type_mount",
            EquipmentIndex.HorseHarness => "General\\EquipmentIcons\\equipment_type_default",
            _ => "General\\EquipmentIcons\\equipment_type_default",
        };
    }

    public void ExecuteAlternateClick()
    {
        _alternateClick?.Invoke(this);
        ClearItem();
    }

    public void ExecuteDragBegin()
    {
        InformationManager.DisplayMessage(new InformationMessage($"ExecuteDragBegin on slot: {EquipmentSlot}"));
        IsDragging = true;
    }

    public void ExecuteDragEnd()
    {
        InformationManager.DisplayMessage(new InformationMessage($"ExecuteDragEnd on slot: {EquipmentSlot}"));
        IsDragging = false;
    }

    public void ExecuteTryEquipItem(ViewModel draggedItemVM, int index)
    {
        InformationManager.DisplayMessage(new InformationMessage($"ExecuteTryEquipItem item: {draggedItemVM.GetType().Name}"));

        ItemObject? item = null;
        if (draggedItemVM is InventorySlotVM inventorySlot)
        {
            InformationManager.DisplayMessage(new InformationMessage($"item name: {inventorySlot.ItemObj?.Name}"));
            item = inventorySlot.ItemObj;

            if (Equipment.IsItemFitsToSlot(EquipmentSlot, item))
            {
                SetItem(new ImageIdentifierVM(item));
            }
        }
        else if (draggedItemVM is EquipmentSlotVM equipmentSlot)
        {
            item = equipmentSlot.ItemObj;
            InformationManager.DisplayMessage(new InformationMessage($"item name: {equipmentSlot.ItemObj?.Name}"));

            // check if picked up and dropped on the same slot
            if (equipmentSlot.EquipmentSlot == EquipmentSlot)
            {
                InformationManager.DisplayMessage(new InformationMessage("Item is already in this slot, returning."));
                return;
            }

            if (Equipment.IsItemFitsToSlot(EquipmentSlot, item))
            {
                // clear current item in slot dragged from
                equipmentSlot.ClearItem();
                SetItem(new ImageIdentifierVM(item));
            }
        }

        // Request to set the item in this slot if matches slot type and allowed (server side eventually)
    }

    public void SetItem(ImageIdentifierVM? newIdentifier)
    {
        ImageIdentifier = newIdentifier;
        ItemObject? newItem = null;

        if (newIdentifier != null && newIdentifier.IsValid && newIdentifier.ImageTypeCode == (int)ImageIdentifierType.Item)
        {
            newItem = Game.Current.ObjectManager.GetObject<ItemObject>(newIdentifier.Id);
        }

        _itemObj = newItem;
        OnPropertyChanged(nameof(ItemObj));
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
            if (value != _imageIdentifier)
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
    public EquipmentIndex EquipmentSlot
    {
        get => _equipmentSlot;
        set
        {
            if (value != _equipmentSlot)
            {
                _equipmentSlot = value;
                OnPropertyChanged(nameof(EquipmentSlot));
            }
        }
    }

    [DataSourceProperty]
    public bool ShouldShowDefaultIcon => (_imageIdentifier == null || !_imageIdentifier.IsValid) || _isDragging;

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
            if (value != _defaultSprite)
            {
                _defaultSprite = value;
                OnPropertyChanged(nameof(DefaultSprite));
            }
        }
    }

    [DataSourceProperty]
    public bool CanAcceptDrag => _imageIdentifier != null && _imageIdentifier.IsValid;

}

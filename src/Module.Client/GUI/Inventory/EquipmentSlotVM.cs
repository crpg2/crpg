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
        LogDebug($"[EquipmentSlotVM] Created slot {crpgSlot}");
    }

    public void ExecuteAlternateClick()
    {
        LogDebug($"[EquipmentSlotVM] Alternate click on slot {CrpgItemSlotIndex}");
        OnSlotAlternateClicked?.Invoke(this);
    }

    public void ExecuteDragBegin()
    {
        LogDebug($"[EquipmentSlotVM] Drag begin on slot {CrpgItemSlotIndex}, item: {ItemObj?.Name}");
        IsDragging = true;
        if (ItemObj != null)
        {
            OnItemDragBegin?.Invoke(ItemObj);
        }

    }

    public void ExecuteDragEnd()
    {
        LogDebug($"[EquipmentSlotVM] Drag end on slot {CrpgItemSlotIndex}");
        IsDragging = false;
        if (ItemObj != null)
        {
            OnItemDragEnd?.Invoke(ItemObj);
        }
    }

    public void ExecuteHoverBegin()
    {
        LogDebug($"[EquipmentSlotVM] Hover Begin {CrpgItemSlotIndex}");
        if (ItemObj != null && IsDragging == true)
        {
            LogDebug($"[EquipmentSlotVM] Hover Begin with item dragged{CrpgItemSlotIndex}");
            // MPLobby\Generic\exit_hover
        }
    }

    public void ExecuteHoverEnd()
    {
        LogDebug($"[EquipmentSlotVM] Hover End {CrpgItemSlotIndex}");
    }

    public void ExecuteTryEquipItem(ViewModel draggedItem, int index)
    {

        LogDebug($"[EquipmentSlotVM] Drop attempt on slot {CrpgItemSlotIndex} with dragged item: {draggedItem.GetType().Name}");
        OnItemDropped?.Invoke(this, draggedItem);
    }

    public void SetItem(ImageIdentifierVM? newIdentifier, ItemObject? itemObj = null, int? userItemId = null)
    {
        ImageIdentifier = newIdentifier;
        ItemObj = itemObj;
        UserItemId = userItemId ?? 0;

        OnPropertyChanged(nameof(ItemObj));
        OnPropertyChanged(nameof(CanAcceptDrag));

        LogDebug($"[EquipmentSlotVM] SetItem called on slot {CrpgItemSlotIndex} with item: {ItemObj?.Name}, userItemId: {UserItemId}");
    }

    public void ClearItem()
    {
        SetItem(new ImageIdentifierVM(ImageIdentifierType.Item));
        LogDebug($"[EquipmentSlotVM] ClearItem called on slot {CrpgItemSlotIndex}");
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

                LogDebug($"[EquipmentSlotVM] ImageIdentifier changed on slot {CrpgItemSlotIndex} to {value?.Id}");
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
                LogDebug($"[EquipmentSlotVM] UserItemId updated on slot {CrpgItemSlotIndex}: {UserItemId}");
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
                LogDebug($"[EquipmentSlotVM] DefaultSprite changed on slot {CrpgItemSlotIndex}: {_defaultSprite}");
            }
        }
    }

    private void LogDebug(string message)
    {
        Debug.Print(message);
        InformationManager.DisplayMessage(new InformationMessage(message));
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
                LogDebug($"[EquipmentSlotVM] CrpgItemSlotIndex updated to {value}");
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
                LogDebug($"[EquipmentSlotVM] IsButtonEnabled changed to {value} on slot {CrpgItemSlotIndex}");
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
}

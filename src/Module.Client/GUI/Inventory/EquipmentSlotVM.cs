using System;
using System.Collections.Generic;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
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
    private int _itemRank = 0;
    private bool _rank1Visible = false;
    private bool _rank2Visible = false;
    private bool _rank3Visible = false;

    public event Action<EquipmentSlotVM, ViewModel>? OnItemDropped;
    public event Action<EquipmentSlotVM>? OnSlotAlternateClicked;
    public event Action<EquipmentSlotVM>? OnSlotClicked;
    public event Action<EquipmentSlotVM>? OnHoverBegin;
    public event Action<EquipmentSlotVM>? OnHoverEnd;
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
        LogDebug($"EquipmentSlotVM: ExecuteAlternateClick()");
        OnSlotAlternateClicked?.Invoke(this);
    }

    public void ExecuteClick()
    {
        LogDebug($"EquipmentSlotVM: ExecuteClick()");
        OnSlotClicked?.Invoke(this);
    }

    public void ExecuteHoverBegin()
    {
        OnHoverBegin?.Invoke(this);
        LogDebug($"EquipmentSlotVM: ExecuteHoverBegin()");
    }

    public void ExecuteHoverEnd()
    {
        OnHoverEnd?.Invoke(this);
        LogDebug($"EquipmentSlotVM: ExecuteHoverEnd()");
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

    public void SetItem(ImageIdentifierVM? newIdentifier, ItemObject? itemObj = null, int? userItemId = null, CrpgUserItemExtended? userItemExtended = null)
    {
        ImageIdentifier = newIdentifier;
        ItemObj = itemObj;
        UserItemId = userItemId ?? 0;
        ItemRank = userItemExtended?.Rank ?? 0;

        OnPropertyChanged(nameof(ItemObj));
        OnPropertyChanged(nameof(CanAcceptDrag));
        OnPropertyChanged(nameof(ItemRank));

        SetItemRankIconsVisible(ItemRank);
    }

    public void ClearItem()
    {
        SetItem(new ImageIdentifierVM(ImageIdentifierType.Item));
    }

    [DataSourceProperty]
    public bool ShouldShowDefaultIcon => (_imageIdentifier == null || !_imageIdentifier.IsValid) || _isDragging;
    [DataSourceProperty]
    public bool CanAcceptDrag => ItemObj != null;
    [DataSourceProperty]
    public int UserItemId { get => _userItemId; set => SetField(ref _userItemId, value, nameof(UserItemId)); }
    [DataSourceProperty]
    public string DefaultSprite { get => _defaultSprite; set => SetField(ref _defaultSprite, value, nameof(DefaultSprite)); }
    [DataSourceProperty]
    public ItemObject? ItemObj { get => _itemObj; set => SetField(ref _itemObj, value, nameof(ItemObj)); }
    [DataSourceProperty]
    public bool IsButtonEnabled { get => _isButtonEnabled; set => SetField(ref _isButtonEnabled, value, nameof(IsButtonEnabled)); }
    public int ItemRank { get => _itemRank; set => SetField(ref _itemRank, value, nameof(ItemRank)); }
    [DataSourceProperty]
    public bool Rank1Visible { get => _rank1Visible; set => SetField(ref _rank1Visible, value, nameof(Rank1Visible)); }
    [DataSourceProperty]
    public bool Rank2Visible { get => _rank2Visible; set => SetField(ref _rank2Visible, value, nameof(Rank2Visible)); }
    [DataSourceProperty]
    public bool Rank3Visible { get => _rank3Visible; set => SetField(ref _rank3Visible, value, nameof(Rank3Visible)); }

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

    private void SetItemRankIconsVisible(int rank)
    {
        InformationManager.DisplayMessage(new InformationMessage($"ItemRank set to {rank}"));
        switch (rank)
        {
            case 1:
                Rank1Visible = true;
                Rank2Visible = false;
                Rank3Visible = false;
                break;
            case 2:
                Rank1Visible = false;
                Rank2Visible = true;
                Rank3Visible = false;
                break;
            case 3:
                Rank1Visible = false;
                Rank2Visible = false;
                Rank3Visible = true;
                break;
            default:
                Rank1Visible = false;
                Rank2Visible = false;
                Rank3Visible = false;
                break;
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

    public void RefreshFromEquipment(Equipment equipment)
    {
        EquipmentIndex eqIndex = ConvertToEquipmentIndex(CrpgItemSlotIndex);

        // Get the EquipmentElement for this slot
        EquipmentElement element = equipment[eqIndex];

        // Extract the ItemObject from the element
        ItemObject? item = element.Item;

        // Update the slot VM
        SetItem(item != null ? new ImageIdentifierVM(ImageIdentifierType.Item) : null, item);

        // Update rank visuals if needed

        if (item != null)
        {
            // ItemRank = item.Level; // or custom rank logic
            // Optionally call SetItemRankIconsVisible(ItemRank);
        }
        else
        {
            // ItemRank = 0;
            // Optionally call SetItemRankIconsVisible(0);
        }
    }

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

    private readonly bool _debugOn = false;
    private void LogDebug(string message)
    {
        if (_debugOn)
        {
            LogDebug(message, Color.White);
        }
    }

    private void LogDebugError(string message)
    {
        LogDebug($"{GetType().Name} {message}", Colors.Red);
    }

    private void LogDebug(string message, Color color)
    {
        Debug.Print(message);
        InformationManager.DisplayMessage(new InformationMessage(message, color));
    }
}

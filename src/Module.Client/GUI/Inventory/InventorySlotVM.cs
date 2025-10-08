using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Common.Network.Armory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class InventorySlotVM : ViewModel
{
    private readonly Action<InventorySlotVM> _onClick;
    private readonly Action<InventorySlotVM> _onHoverEnd;
    private readonly Action<InventorySlotVM>? _onDragBegin;
    private readonly Action<InventorySlotVM>? _onDragEnd;

    private string _itemName;
    private string _defaultSprite;
    private bool _showDefaultIcon;
    private int _itemQuantity;
    private string _quantityText;
    private int _userItemId;
    private bool _isEquipped;
    private int _itemRank = 0;
    private bool _isArmoryItem;
    private bool _isDraggable = true;

    private CrpgUserItemExtended? _userItemEx;

    private ImageIdentifierVM _imageIdentifier;

    private ItemRankIconVM _itemRankIcon;
    private ItemArmoryIconVM? _itemArmoryIcon;
    private string _id;

    public ItemObject ItemObj { get; }
    internal CrpgCharacterLoadoutBehaviorClient? UserLoadoutBehavior { get; set; }
    private readonly CrpgClanArmoryClient? _clanArmory;

    public InventorySlotVM(ItemObject item,
         Action<InventorySlotVM> onClick,
         Action<InventorySlotVM> onHoverEnd,
         Action<InventorySlotVM> onDragBegin,
         Action<InventorySlotVM> onDragEnd,
         int quantity = 1,
         CrpgUserItemExtended? userItemExtended = null)
    {
        UserLoadoutBehavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();
        /*
        if (_clanArmory is null)
        {
            _clanArmory = new CrpgClanArmoryClient();
            Mission.Current?.AddMissionBehavior(_clanArmory);
        }
        */

        ItemObj = item;
        if (item != null)
        {
            _itemName = item.Name?.ToString() ?? "Item";
            _imageIdentifier = new ImageIdentifierVM(item);
            _id = item.StringId;
            _showDefaultIcon = false;
            _quantityText = quantity > 1 ? quantity.ToString() : string.Empty;
            _defaultSprite = string.Empty;
            UserItemEx = userItemExtended;
            _userItemId = userItemExtended?.Id ?? -1;
            _itemRank = userItemExtended?.Rank ?? 0;
            _itemRankIcon = new ItemRankIconVM(_itemRank);
            _itemArmoryIcon = new ItemArmoryIconVM();
            _isArmoryItem = userItemExtended?.IsArmoryItem ?? false;
            _isDraggable = true;
            _onClick = onClick;
            _onHoverEnd = onHoverEnd;
            _onDragBegin = onDragBegin;
            _onDragEnd = onDragEnd;

            if (UserLoadoutBehavior is null)
            {
                InformationManager.DisplayMessage(new InformationMessage("CrpgCharacterLoadoutBehaviorClient is required but not found in current mission", Colors.Red));
                return;
            }

            UserLoadoutBehavior.OnEquipmentSlotUpdated += HandleUpdateEvent;
            UserLoadoutBehavior.OnUserInventoryUpdated += HandleUpdateEvent;
            UserLoadoutBehavior.OnUserCharacterEquippedItemsUpdated += HandleUpdateEvent;

            if (_clanArmory is not null)
            {
                _clanArmory.OnClanArmoryUpdated += HandleUpdateEvent;
                _clanArmory.OnArmoryActionUpdated += HandleUpdateEvent;
            }

            if (_isArmoryItem)
            {
                // IsDraggable = !UserLoadoutBehavior.IsArmoryItemOwner(_userItemId); // dont let equip if armory item and owner
                IsDraggable = CanDragSlot();
                _itemArmoryIcon?.UpdateItemArmoryIconFromItem(_userItemId);
            }

            CheckItemEquipped();
        }
        else
        {
            _itemName = string.Empty;
            _imageIdentifier = new ImageIdentifierVM(); // empty
            _itemRankIcon = new ItemRankIconVM();
            _itemArmoryIcon = new ItemArmoryIconVM();
            _id = string.Empty;
            _showDefaultIcon = true;
            _quantityText = string.Empty;
            _defaultSprite = "general_placeholder";
            _userItemId = -1;
            _itemRank = 0;
            _isArmoryItem = false;
            _isDraggable = false;
            _onClick = _ => { }; // no-op
            _onHoverEnd = _ => { }; // no-op
            _onDragBegin = _ => { }; // no-op;
            _onDragEnd = _ => { }; // no-op;
        }
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
        if (UserLoadoutBehavior is not null)
        {
            UserLoadoutBehavior.OnEquipmentSlotUpdated -= HandleUpdateEvent;
            UserLoadoutBehavior.OnUserInventoryUpdated -= HandleUpdateEvent;
            UserLoadoutBehavior.OnUserCharacterEquippedItemsUpdated -= HandleUpdateEvent;
        }

        if (_clanArmory is not null)
        {
            _clanArmory.OnClanArmoryUpdated -= HandleUpdateEvent;
            _clanArmory.OnArmoryActionUpdated -= HandleUpdateEvent;
        }
    }

    public void HandleUpdateEvent(CrpgItemSlot slot)
    {
        HandleUpdateEvent();
    }

    public void HandleUpdateEvent(ClanArmoryActionType action, int userItemId)
    {
        HandleUpdateEvent();
    }

    public void HandleUpdateEvent()
    {
        HandleUpdateEvent(UserItemId);
    }

    internal void HandleUpdateEvent(int userItemId)
    {
        // InformationManager.DisplayMessage(new InformationMessage($"InventorySlotVM: HandleUpdateEvent({userItemId})"));
        if (UserLoadoutBehavior is not null)
        {
            // Try to fetch the latest version of this item from client behavior, not API
            var latestItem = UserLoadoutBehavior.GetCrpgUserItem(userItemId);

            // If it doesn’t exist anymore, clear it
            if (latestItem == null)
            {
                // InformationManager.DisplayMessage(new InformationMessage($"InventorySlotVM: HandleUpdateEvent({userItemId}) -- latestItem is null", Colors.Red));
                UserItemEx = null;
                return;
            }

            // If this slot is tracking the same item, update it
            if (UserItemEx?.Id == userItemId)
            {
                // InformationManager.DisplayMessage(new InformationMessage($"InventorySlotVM: HandleUpdateEvent({userItemId}) setting UserItemEx"));
                UserItemEx = latestItem;
            }
        }
    }

    private void ApplyUserItemEx(CrpgUserItemExtended? uItem)
    {
        // InformationManager.DisplayMessage(new InformationMessage($"InventorySlotVM: ApplyUserItemEx() {uItem?.ItemId}"));
        if (uItem == null)
        {
            UserItemId = -1;
            ItemRank = 0;
            IsArmoryItem = false;
            IsEquipped = false;
            IsDraggable = true;
            QuantityText = string.Empty;
            // SetItemRankIconsVisible(0);
            ItemRankIcon = new ItemRankIconVM(_itemRank);
            ItemArmoryIcon = new ItemArmoryIconVM();
            return;
        }

        UserItemId = uItem.Id;
        ItemRank = uItem.Rank;
        IsArmoryItem = uItem.IsArmoryItem;
        IsDraggable = CanDragSlot();
        IsEquipped = UserLoadoutBehavior?.IsItemEquipped(uItem.Id) ?? false;

        // If you have a stack amount
        // ItemQuantity = uItem.Quantity;
        // QuantityText = ItemQuantity > 1 ? ItemQuantity.ToString() : string.Empty;
        ItemRankIcon = new ItemRankIconVM(_itemRank);
        ItemArmoryIcon?.UpdateItemArmoryIconFromItem(uItem.Id);
    }

    private bool CanDragSlot()
    {
        if (!IsArmoryItem || UserItemEx is null || UserLoadoutBehavior is null)
        {
            return true;
        }

        if (_clanArmory is not null)
        {
            if (!_clanArmory.GetCrpgUserItemArmoryStatus(UserItemEx.Id, out var status))
            {
                return true;
            }

            return status switch
            {
                CrpgGameArmoryItemStatus.YoursAvailable => false,
                CrpgGameArmoryItemStatus.YoursBorrowed => false,
                CrpgGameArmoryItemStatus.NotYoursAvailible => false,
                CrpgGameArmoryItemStatus.NotYoursBorrowed => false,
                _ => true,
            };
        }

        return true;
    }

    [DataSourceProperty]
    public CrpgUserItemExtended? UserItemEx
    {
        get => _userItemEx;
        set
        {
            if (SetField(ref _userItemEx, value, nameof(UserItemEx)))
            {
                ApplyUserItemEx(_userItemEx);
            }
            else
            {
                // Optional: still run if same object was assigned
                ApplyUserItemEx(_userItemEx);
            }
        }
    }

    [DataSourceProperty]
    public int UserItemId
    {
        get => _userItemId; set
        {
            if (_userItemId != value)
            {
                _userItemId = value;
                OnPropertyChanged(nameof(UserItemId));
            }
        }
    }

    [DataSourceProperty]
    public bool IsDraggable { get => _isDraggable; set => SetField(ref _isDraggable, value, nameof(IsDraggable)); }
    [DataSourceProperty]
    public bool IsArmoryItem { get => _isArmoryItem; set => SetField(ref _isArmoryItem, value, nameof(IsArmoryItem)); }
    [DataSourceProperty]
    public bool IsEquipped { get => _isEquipped; set => SetField(ref _isEquipped, value, nameof(IsEquipped)); }
    [DataSourceProperty]
    public int ItemRank { get => _itemRank; set => SetField(ref _itemRank, value, nameof(ItemRank)); }

    [DataSourceProperty]
    public string ItemName { get => _itemName; set => SetField(ref _itemName, value, nameof(ItemName)); }

    [DataSourceProperty]
    public string DefaultSprite { get => _defaultSprite; set => SetField(ref _defaultSprite, value, nameof(DefaultSprite)); }

    [DataSourceProperty]
    public bool ShowDefaultIcon { get => _showDefaultIcon; set => SetField(ref _showDefaultIcon, value, nameof(ShowDefaultIcon)); }

    [DataSourceProperty]
    public string QuantityText { get => _quantityText; set => SetField(ref _quantityText, value, nameof(QuantityText)); }

    [DataSourceProperty]
    public ImageIdentifierVM ImageIdentifier { get => _imageIdentifier; set => SetField(ref _imageIdentifier, value, nameof(ImageIdentifier)); }

    [DataSourceProperty]
    public string Id { get => _id; set => SetField(ref _id, value, nameof(Id)); }

    [DataSourceProperty]
    public ItemRankIconVM ItemRankIcon { get => _itemRankIcon; set => SetField(ref _itemRankIcon, value, nameof(ItemRankIcon)); }

    [DataSourceProperty]
    public ItemArmoryIconVM? ItemArmoryIcon { get => _itemArmoryIcon; set => SetField(ref _itemArmoryIcon, value, nameof(ItemArmoryIcon)); }

    [DataSourceProperty]
    public int ItemQuantity
    {
        get => _itemQuantity;
        set
        {
            if (SetField(ref _itemQuantity, value, nameof(ItemQuantity)))
            {
                QuantityText = value > 1 ? value.ToString() : string.Empty;
            }
        }
    }

    public void ExecuteDragBegin()
    {
        if (ItemObj != null)
        {
            InformationManager.DisplayMessage(new InformationMessage($"InventorySlotVM: ExecuteDragBegin()"));
            // OnItemDragBegin?.Invoke(ItemObj);
            _onDragBegin?.Invoke(this);
        }
    }

    public void ExecuteDragEnd()
    {
        if (ItemObj != null)
        {
            _onDragEnd?.Invoke(this);
        }
    }

    public void ExecuteClick()
    {
        if (ItemObj != null)
        {
            _onClick?.Invoke(this);
        }
    }

    public void ExecuteHoverBegin()
    {
        // _onHoverEnd?.Invoke(this);
        // InformationManager.DisplayMessage(new InformationMessage($"InventorySlotVM: ExecuteHoverBegin()"));
    }

    public void ExecuteHoverEnd()
    {
        _onHoverEnd?.Invoke(this);
    }

    private void CheckItemEquipped()
    {
        if (UserLoadoutBehavior is null || UserLoadoutBehavior.UserInventoryItems is null || UserLoadoutBehavior.EquippedItems is null)
        {
            return;
        }

        var equipped = UserLoadoutBehavior?.EquippedItems
        .FirstOrDefault(e => e.UserItem.Id == _userItemId);

        if (equipped != null)
        {
            // Item is equipped
            // InformationManager.DisplayMessage(new InformationMessage($"Item {_itemName} is equipped in slot {equipped.Slot}"));

            // Example: you could set a flag for UI
            IsEquipped = true;
        }
        else
        {
            // Item is not equipped
            IsEquipped = false;
        }
    }
}

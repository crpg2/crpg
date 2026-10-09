using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Crpg.Module.Common.Network.Armory;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class InventorySlotVM : ViewModel
{
    private readonly Action<InventorySlotVM>? _onClick;
    private readonly Action<InventorySlotVM>? _onHoverEnd;
    private readonly Action<InventorySlotVM>? _onDragBegin;
    private readonly Action<InventorySlotVM>? _onDragEnd;
    private readonly Action<InventorySlotVM>? _onAlternateClick;
    private readonly Func<InventoryGridVM.InventorySection>? _getActiveSection;

    private readonly CrpgCharacterLoadoutBehaviorClient? _userLoadout;
    private readonly CrpgClanArmoryClient? _clanArmory;
    private readonly CrpgTeamInventoryClient? _teamInventory;

    private CrpgUserItemExtended? _userItemEx;

    public ItemObject ItemObj { get; }

    public InventorySlotVM(
        ItemObject item,
        Action<InventorySlotVM> onClick,
        Action<InventorySlotVM> onHoverEnd,
        Action<InventorySlotVM> onDragBegin,
        Action<InventorySlotVM> onDragEnd,
        Action<InventorySlotVM>? onAlternateClick = null,
        int quantity = 1,
        CrpgUserItemExtended? userItemExtended = null,
        Func<InventoryGridVM.InventorySection>? getActiveSection = null)
    {
        _userLoadout = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        _clanArmory = Mission.Current?.GetMissionBehavior<CrpgClanArmoryClient>();
        _teamInventory = Mission.Current?.GetMissionBehavior<CrpgTeamInventoryClient>();
        _getActiveSection = getActiveSection;
        _onClick = onClick;
        _onHoverEnd = onHoverEnd;
        _onDragBegin = onDragBegin;
        _onDragEnd = onDragEnd;
        _onAlternateClick = onAlternateClick;

        ItemObj = item;

        if (item != null)
        {
            // Set static item data -- never changes
            ItemName = item.Name?.ToString() ?? "Item";
            ImageIdentifier = new ItemImageIdentifierVM(item);
            Id = item.StringId;
            ShowDefaultIcon = false;
            DefaultSprite = string.Empty;

            ItemQuantity = quantity;

            // Apply user item data (sets UserItemId, rank, armory state, etc.)
            // Set field directly to avoid triggering ApplyUserItemEx before subscriptions
            _userItemEx = userItemExtended; // sets backing field for UserItemEx
            ApplyUserItemEx(userItemExtended);

            SubscribeEvents();
        }
        else
        {
            SetEmptyDefaults();
        }
    }

    public override void OnFinalize()
    {
        base.OnFinalize();

        if (_userLoadout is not null)
        {
            _userLoadout.OnEquipmentSlotUpdated -= OnSlotUpdated;
            _userLoadout.OnUserInventoryUpdated -= RefreshSlotState;
            _userLoadout.OnUserCharacterEquippedItemsUpdated -= RefreshSlotState;
        }

        if (_clanArmory is not null)
        {
            _clanArmory.OnClanArmoryUpdated -= RefreshSlotState;
            _clanArmory.OnArmoryActionUpdated -= OnArmoryActionUpdated;
        }

        if (_teamInventory is not null && IsTeamInventoryItem)
        {
            _teamInventory.OnEquippedTeamItemsUpdated -= RefreshSlotState;
            _teamInventory.OnTeamItemsUpdated -= RefreshSlotState;
        }
    }

    private void SubscribeEvents()
    {
        if (_userLoadout is not null)
        {
            _userLoadout.OnEquipmentSlotUpdated += OnSlotUpdated;
            _userLoadout.OnUserInventoryUpdated += RefreshSlotState;
            _userLoadout.OnUserCharacterEquippedItemsUpdated += RefreshSlotState;
        }

        if (_clanArmory is not null)
        {
            _clanArmory.OnClanArmoryUpdated += RefreshSlotState;
            _clanArmory.OnArmoryActionUpdated += OnArmoryActionUpdated;
        }

        if (_teamInventory is not null && IsTeamInventoryItem)
        {
            _teamInventory.OnEquippedTeamItemsUpdated += RefreshSlotState;
            _teamInventory.OnTeamItemsUpdated += RefreshSlotState;
        }
    }

    private void SetEmptyDefaults()
    {
        ItemName = string.Empty;
        ImageIdentifier = new GenericImageIdentifierVM(null);
        ItemRankIcon = new ItemRankIconVM();
        ItemArmoryIcon = new ItemArmoryIconVM();
        Id = string.Empty;
        ShowDefaultIcon = true;
        ItemQuantity = 0;
        QuantityText = string.Empty;
        DefaultSprite = "general_placeholder";
        UserItemId = -1;
        ItemRank = 0;
        IsArmoryItem = false;
        IsTeamInventoryItem = false;
        IsDraggable = false;
        IsEquipped = false;
        QuantityTextVisible = false;
    }

    // Event adapters — match required signatures, delegate to RefreshSlotState
    private void OnSlotUpdated(CrpgItemSlot slot) => RefreshSlotState();
    private void OnArmoryActionUpdated(ClanArmoryActionType action, int userItemId) => RefreshSlotState();

    private void RefreshSlotState()
    {
        bool isTeamSection = _getActiveSection?.Invoke() == InventoryGridVM.InventorySection.TeamInventory;

        if (isTeamSection && IsTeamInventoryItem)
        {
            if (_teamInventory is not null)
            {
                var teamItem = _teamInventory.FindTeamInventoryItem(UserItemEx?.ItemId ?? string.Empty);
                if (teamItem is not null)
                {
                    ItemQuantity = teamItem.Quantity;
                    IsDraggable = CanDragSlot();
                    IsEquipped = IsItemEquipped();
                }
            }

            return;
        }

        if (_userLoadout is null)
        {
            return;
        }

        var latestItem = _userLoadout.GetCrpgUserItem(UserItemId);
        if (latestItem == null)
        {
            UserItemEx = null;
            return;
        }

        if (UserItemEx?.Id == UserItemId)
        {
            UserItemEx = latestItem;
        }
    }

    private void ApplyUserItemEx(CrpgUserItemExtended? uItem)
    {
        if (uItem == null)
        {
            UserItemId = -1;
            ItemRank = 0;
            IsArmoryItem = false;
            IsTeamInventoryItem = false;
            IsEquipped = false;
            IsDraggable = true;
            QuantityText = string.Empty;
            QuantityTextVisible = false;
            ItemRankIcon = new ItemRankIconVM(0);
            ItemArmoryIcon = new ItemArmoryIconVM();
            return;
        }

        UserItemId = uItem.Id;
        ItemRank = uItem.Rank;
        IsArmoryItem = uItem.IsArmoryItem;
        IsTeamInventoryItem = CrpgTeamInventoryClient.IsTeamInventoryItem(uItem.Id);
        IsDraggable = CanDragSlot();
        IsEquipped = IsItemEquipped();
        QuantityText = ItemQuantity > 1 ? ItemQuantity.ToString() : string.Empty;
        QuantityTextVisible = ItemQuantity > 1;
        ItemRankIcon = new ItemRankIconVM(ItemRank);
        ItemArmoryIcon = new ItemArmoryIconVM();
        ItemArmoryIcon.UpdateItemArmoryIconFromItem(uItem.Id);
    }

    private bool IsItemEquipped()
    {
        bool isTeamSection = _getActiveSection?.Invoke() == InventoryGridVM.InventorySection.TeamInventory;

        if (isTeamSection) // team inventory items selected
        {
            return _teamInventory?.IsTeamItemEquipped(UserItemEx?.ItemId ?? string.Empty) ?? false;
        }

        // not team inventory, check normal equipped status
        return _userLoadout?.EquippedItems?.Any(e => e.UserItem.Id == UserItemId) ?? false;
    }

    private bool CanDragSlot()
    {
        if (IsTeamInventoryItem)
        {
            CrpgTeamInventoryItem? teamItem = _teamInventory?.FindTeamInventoryItem(UserItemEx?.ItemId ?? string.Empty);
            return teamItem?.Quantity > 0;
        }

        if (!IsArmoryItem || UserItemEx is null || _clanArmory is null)
        {
            return true;
        }

        return _clanArmory.GetCrpgUserItemArmoryStatus(UserItemEx.Id, out var status)
            && status is CrpgGameArmoryItemStatus.BorrowedByYou;
    }

    // ===== Properties =====

    [DataSourceProperty]
    public CrpgUserItemExtended? UserItemEx
    {
        get => _userItemEx;
        set
        {
            if (_userItemEx != value)
            {
                _userItemEx = value;
                OnPropertyChanged(nameof(UserItemEx));
                ApplyUserItemEx(_userItemEx);
            }
        }
    }

    [DataSourceProperty]
    public int UserItemId { get; set => SetField(ref field, value, nameof(UserItemId)); }

    [DataSourceProperty]
    public bool IsDraggable { get; set => SetField(ref field, value, nameof(IsDraggable)); }

    [DataSourceProperty]
    public bool IsArmoryItem { get; set => SetField(ref field, value, nameof(IsArmoryItem)); }

    [DataSourceProperty]
    public bool IsTeamInventoryItem { get; set => SetField(ref field, value, nameof(IsTeamInventoryItem)); }

    [DataSourceProperty]
    public bool IsEquipped { get; set => SetField(ref field, value, nameof(IsEquipped)); }

    [DataSourceProperty]
    public int ItemRank { get; set => SetField(ref field, value, nameof(ItemRank)); }

    [DataSourceProperty]
    public string ItemName { get; set => SetField(ref field, value, nameof(ItemName)); } = string.Empty;

    [DataSourceProperty]
    public string DefaultSprite { get; set => SetField(ref field, value, nameof(DefaultSprite)); } = string.Empty;

    [DataSourceProperty]
    public bool ShowDefaultIcon { get; set => SetField(ref field, value, nameof(ShowDefaultIcon)); }

    [DataSourceProperty]
    public string QuantityText { get; set => SetField(ref field, value, nameof(QuantityText)); } = string.Empty;

    [DataSourceProperty]
    public ImageIdentifierVM ImageIdentifier { get; set => SetField(ref field, value, nameof(ImageIdentifier)); } = default!;

    [DataSourceProperty]
    public string Id { get; set => SetField(ref field, value, nameof(Id)); } = string.Empty;

    [DataSourceProperty]
    public ItemRankIconVM ItemRankIcon { get; set => SetField(ref field, value, nameof(ItemRankIcon)); } = default!;

    [DataSourceProperty]
    public ItemArmoryIconVM? ItemArmoryIcon { get; set => SetField(ref field, value, nameof(ItemArmoryIcon)); }

    [DataSourceProperty]
    public int ItemQuantity
    {
        get;
        set
        {
            if (SetField(ref field, value, nameof(ItemQuantity)))
            {
                QuantityText = value > 1 ? value.ToString() : string.Empty;
                QuantityTextVisible = value > 1;
            }
        }
    }

    [DataSourceProperty]
    public bool QuantityTextVisible { get; set => SetField(ref field, value, nameof(QuantityTextVisible)); }

    // ===== Event handlers for UI actions =====

    public void ExecuteDragBegin()
    {
        if (ItemObj != null)
        {
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

    public void ExecuteAlternateClick()
    {
        if (ItemObj != null)
        {
            _onAlternateClick?.Invoke(this);
        }
    }

    public void ExecuteHoverBegin()
    {
        // _onHoverBegin?.Invoke(this);
    }

    public void ExecuteHoverEnd()
    {
        _onHoverEnd?.Invoke(this);
    }
}

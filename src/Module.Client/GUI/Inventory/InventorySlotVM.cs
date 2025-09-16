using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common;
using Messages.FromClient.ToLobbyServer;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class InventorySlotVM : ViewModel
{
    private readonly Action<InventorySlotVM> _onClick;
    private readonly Action<InventorySlotVM> _onHoverEnd;

    private string _itemName;
    private string _defaultSprite;
    private bool _showDefaultIcon;
    private int _itemQuantity;
    private string _quantityText;
    private int _userItemId;
    private bool _isEquipped;
    private int _itemRank = 0;
    private bool _rank1Visible = false;
    private bool _rank2Visible = false;
    private bool _rank3Visible = false;

    private ImageIdentifierVM _imageIdentifier;
    private string _id;

    public ItemObject ItemObj { get; }
    public event Action<ItemObject>? OnItemDragBegin;
    public event Action<ItemObject>? OnItemDragEnd;
    public event Action<ItemObject>? OnItemClick;

    public InventorySlotVM(ItemObject item, Action<InventorySlotVM> onClick, Action<InventorySlotVM> onHoverEnd, int quantity = 1, int userItemId = -1)
    {
        ItemObj = item;
        if (item != null)
        {
            _itemName = item.Name?.ToString() ?? "Item";
            _imageIdentifier = new ImageIdentifierVM(item);
            _id = item.StringId;
            _showDefaultIcon = false;
            _quantityText = quantity > 1 ? quantity.ToString() : string.Empty;
            _defaultSprite = string.Empty;
            _userItemId = userItemId;
            _itemRank = 0;
            _onClick = onClick;
            _onHoverEnd = onHoverEnd;

            var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
            if (behavior == null || behavior.UserInventoryItems == null)
            {
                return;
            }

            var userItem = behavior.UserInventoryItems.FirstOrDefault(x => x.Id == _userItemId);
            if (userItem != null)
            {
                _itemRank = userItem.Rank;
            }

            behavior.OnSlotUpdated += HandleUpdateEvent;
            behavior.OnUserInventoryUpdated += HandleUpdateEvent;
            behavior.OnUserCharacterEquippedItemsUpdated += HandleUpdateEvent;

            SetItemRankIconsVisible(_itemRank);

            CheckItemEquipped();
        }
        else
        {
            _itemName = string.Empty;
            _imageIdentifier = new ImageIdentifierVM(); // empty
            _id = string.Empty;
            _showDefaultIcon = true;
            _quantityText = string.Empty;
            _defaultSprite = "general_placeholder";
            _userItemId = -1;
            _itemRank = 0;
            _onClick = _ => { }; // no-op
            _onHoverEnd = _ => { }; // no-op
        }
    }

    public override void OnFinalize()
    {
        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior != null)
        {
            behavior.OnSlotUpdated -= HandleUpdateEvent;
            behavior.OnUserInventoryUpdated -= HandleUpdateEvent;
            behavior.OnUserCharacterEquippedItemsUpdated -= HandleUpdateEvent;
        }
    }

    public void HandleUpdateEvent(CrpgItemSlot slot)
    {
        HandleUpdateEvent();
    }

    public void HandleUpdateEvent()
    {
        CheckItemEquipped();
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
    public bool IsEquipped { get => _isEquipped; set => SetField(ref _isEquipped, value, nameof(IsEquipped)); }
    [DataSourceProperty]
    public int ItemRank { get => _itemRank; set => SetField(ref _itemRank, value, nameof(ItemRank)); }
    [DataSourceProperty]
    public bool Rank1Visible { get => _rank1Visible; set => SetField(ref _rank1Visible, value, nameof(Rank1Visible)); }
    [DataSourceProperty]
    public bool Rank2Visible { get => _rank2Visible; set => SetField(ref _rank2Visible, value, nameof(Rank2Visible)); }
    [DataSourceProperty]
    public bool Rank3Visible { get => _rank3Visible; set => SetField(ref _rank3Visible, value, nameof(Rank3Visible)); }
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
            OnItemDragBegin?.Invoke(ItemObj);
        }
    }

    public void ExecuteDragEnd()
    {
        if (ItemObj != null)
        {
            InformationManager.DisplayMessage(new InformationMessage($"InventorySlotVM: ExecuteDragdEnd()"));

            OnItemDragEnd?.Invoke(ItemObj);
        }
    }

    public void ExecuteClick()
    {
        if (ItemObj != null)
        {
            InformationManager.DisplayMessage(new InformationMessage($"InventorySlotVM: ExecuteClick()"));
            // OnItemClick?.Invoke(ItemObj);
            _onClick?.Invoke(this);
        }
    }

    public void ExecuteHoverBegin()
    {
        // _onHoverEnd?.Invoke(this);
        // InformationManager.DisplayMessage(new InformationMessage($"ExecuteHoverBegin()"));
    }

    public void ExecuteHoverEnd()
    {
        InformationManager.DisplayMessage(new InformationMessage($"ExecuteHoverEnd()"));
        _onHoverEnd?.Invoke(this);

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

    private void CheckItemEquipped()
    {
        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null || behavior.UserInventoryItems == null)
        {
            return;
        }

        var equipped = behavior.EquippedItems
        .FirstOrDefault(e => e.UserItem.Id == _userItemId);

        if (equipped != null)
        {
            // Item is equipped
            InformationManager.DisplayMessage(new InformationMessage($"Item {_itemName} is equipped in slot {equipped.Slot}"));

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

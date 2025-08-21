using Crpg.Module.Api.Models.Characters;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class InventorySlotVM : ViewModel
{
    private string _itemName;
    private string _defaultSprite;
    private bool _showDefaultIcon;
    private int _itemQuantity;
    private string _quantityText;
    private int _userItemId;

    private ImageIdentifierVM _imageIdentifier;
    private string _id;

    public ItemObject ItemObj { get; }
    public event Action<ItemObject>? OnItemDragBegin;
    public event Action<ItemObject>? OnItemDragEnd;

    public InventorySlotVM(ItemObject item, int quantity = 1, int userItemId = -1)
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

    [DataSourceProperty]
    public string ItemName
    {
        get => _itemName;
        set => SetField(ref _itemName, value, nameof(ItemName));
    }

    [DataSourceProperty]
    public string DefaultSprite
    {
        get => _defaultSprite;
        set => SetField(ref _defaultSprite, value, nameof(DefaultSprite));
    }

    [DataSourceProperty]
    public bool ShowDefaultIcon
    {
        get => _showDefaultIcon;
        set => SetField(ref _showDefaultIcon, value, nameof(ShowDefaultIcon));
    }

    [DataSourceProperty]
    public string QuantityText
    {
        get => _quantityText;
        set => SetField(ref _quantityText, value, nameof(QuantityText));
    }

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

    [DataSourceProperty]
    public ImageIdentifierVM ImageIdentifier
    {
        get => _imageIdentifier;
        set => SetField(ref _imageIdentifier, value, nameof(ImageIdentifier));
    }

    [DataSourceProperty]
    public string Id
    {
        get => _id;
        set => SetField(ref _id, value, nameof(Id));
    }

    public void ExecuteDragBegin()
    {
        if (ItemObj != null)
        {
            OnItemDragBegin?.Invoke(ItemObj);
        }
    }

    public void ExecuteDragEnd()
    {
        if (ItemObj != null)
        {
            OnItemDragEnd?.Invoke(ItemObj);
        }
    }
}

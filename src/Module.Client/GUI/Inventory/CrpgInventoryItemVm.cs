using JetBrains.Annotations;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

internal class CrpgInventoryItemVm : ViewModel
{
    private static readonly string[] RankColors = ["#FFFFFFFF", "#4ADE80FF", "#60A5FAFF", "#C084FCFF"];

    private string _name = string.Empty;
    private string _nameColor = RankColors[0];
    private bool _isBroken;
    private bool _isEquipped;
    private ImageIdentifierVM _imageIdentifier = new GenericImageIdentifierVM(null);

    public ItemObject Item { get; }
    public int Rank { get; }

    /// <summary>Set by the parent <see cref="CrpgInventoryVm"/> to handle auto-equip on double-click.</summary>
    public Action<CrpgInventoryItemVm>? AutoEquipAction { get; set; }

    public CrpgInventoryItemVm(ItemObject item, int rank, bool isBroken)
    {
        Item = item;
        Rank = rank;
        IsBroken = isBroken;
        NameColor = rank >= 0 && rank < RankColors.Length ? RankColors[rank] : RankColors[0];
        Name = item.Name.ToString();
        ImageIdentifier = new ItemImageIdentifierVM(item);
    }

    /// <summary>Called from XML on double-click. Delegates to parent VM via callback.</summary>
    public void ExecuteAutoEquip()
    {
        AutoEquipAction?.Invoke(this);
    }

    [DataSourceProperty]
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string NameColor
    {
        get => _nameColor;
        set
        {
            _nameColor = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsBroken
    {
        get => _isBroken;
        set
        {
            _isBroken = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsEquipped
    {
        get => _isEquipped;
        set
        {
            _isEquipped = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public ImageIdentifierVM ImageIdentifier
    {
        get => _imageIdentifier;
        set
        {
            _imageIdentifier = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string TypeName => Item.ItemType.ToString();

    /// <summary>Used by <see cref="CrpgInventoryItemWidget"/> for drag identification.</summary>
    [DataSourceProperty]
    public int ItemTypeInt => (int)Item.ItemType;

    /// <summary>MBGUID InternalValue for widget-level item identification.</summary>
    [DataSourceProperty]
    public string MbGuid => Item.Id.InternalValue.ToString();
}

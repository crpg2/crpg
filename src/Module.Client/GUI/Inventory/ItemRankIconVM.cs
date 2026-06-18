using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class ItemRankIconVM : ViewModel
{
    [DataSourceProperty]
    public int ItemRank
    {
        get;
        set
        {
            if (SetField(ref field, value, nameof(ItemRank)))
            {
                Rank1Visible = value == 1;
                Rank2Visible = value == 2;
                Rank3Visible = value == 3;
            }
        }
    }

    [DataSourceProperty]
    public bool Rank1Visible { get; set => SetField(ref field, value, nameof(Rank1Visible)); }
    [DataSourceProperty]
    public bool Rank2Visible { get; set => SetField(ref field, value, nameof(Rank2Visible)); }
    [DataSourceProperty]
    public bool Rank3Visible { get; set => SetField(ref field, value, nameof(Rank3Visible)); }

    public ItemRankIconVM(int initialRank = 0)
    {
        ItemRank = initialRank;
    }
}

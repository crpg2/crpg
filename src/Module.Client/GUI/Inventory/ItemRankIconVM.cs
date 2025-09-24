using TaleWorlds.Library;

namespace Crpg.Module.GUI.Inventory;

public class ItemRankIconVM : ViewModel
{
    private int _itemRank = 0;
    private bool _rank1Visible;
    private bool _rank2Visible;
    private bool _rank3Visible;

    [DataSourceProperty]
    public int ItemRank
    {
        get => _itemRank;
        set
        {
            if (SetField(ref _itemRank, value, nameof(ItemRank)))
            {
                SetItemRankIconsVisible(value);
            }
        }
    }

    [DataSourceProperty]
    public bool Rank1Visible { get => _rank1Visible; set => SetField(ref _rank1Visible, value, nameof(Rank1Visible)); }
    [DataSourceProperty]
    public bool Rank2Visible { get => _rank2Visible; set => SetField(ref _rank2Visible, value, nameof(Rank2Visible)); }
    [DataSourceProperty]
    public bool Rank3Visible { get => _rank3Visible; set => SetField(ref _rank3Visible, value, nameof(Rank3Visible)); }

    public ItemRankIconVM(int initialRank = 0)
    {
        ItemRank = initialRank;
    }

    private void SetItemRankIconsVisible(int rank)
    {
        InformationManager.DisplayMessage(new InformationMessage($"ItemRankIconVM set to {rank}"));
        // Rank 0 no image
        Rank1Visible = false;
        Rank2Visible = false;
        Rank3Visible = false;

        switch (rank)
        {
            case 1:
                Rank1Visible = true;
                break;
            case 2:
                Rank2Visible = true;
                break;
            case 3:
                Rank3Visible = true;
                break;
        }
    }
}

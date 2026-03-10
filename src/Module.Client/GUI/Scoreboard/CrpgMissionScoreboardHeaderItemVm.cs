using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;

namespace Crpg.Module.GUI.Scoreboard;

public class CrpgMissionScoreboardHeaderItemVm : BindingListStringItem
{
    private readonly CrpgScoreboardSideVM _side;

    private string _headerId = string.Empty;

    private bool _isIrregularStat;

    private bool _isAvatarStat;

    [DataSourceProperty]
    public string HeaderID
    {
        get
        {
            return _headerId;
        }
        set
        {
            if (value != _headerId)
            {
                _headerId = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsIrregularStat
    {
        get
        {
            return _isIrregularStat;
        }
        set
        {
            if (value != _isIrregularStat)
            {
                _isIrregularStat = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsAvatarStat
    {
        get
        {
            return _isAvatarStat;
        }
        set
        {
            if (value != _isAvatarStat)
            {
                _isAvatarStat = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MissionScoreboardPlayerSortControllerVM? PlayerSortController => _side.PlayerSortController;

    public CrpgMissionScoreboardHeaderItemVm(CrpgScoreboardSideVM side, string headerId, string value, bool isAvatarStat, bool isIrregularStat)
        : base(value)
    {
        _side = side;
        HeaderID = headerId;
        IsAvatarStat = isAvatarStat;
        IsIrregularStat = isIrregularStat;
    }
}

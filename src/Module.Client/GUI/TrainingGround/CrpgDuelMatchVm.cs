using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using MathF = TaleWorlds.Library.MathF;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.HUDExtensions;

public class CrpgDuelMatchVm : ViewModel
{
    private float _prepTimeRemaining;
    private TextObject _duelCountdownText;
    private bool _isEnabled;
    private bool _isPreparing;
    private string _countdownMessage = string.Empty;
    private string _score = string.Empty;
    private int _firstPlayerScore;
    private int _secondPlayerScore;
    private MPPlayerVM _firstPlayer = null!;
    private MPPlayerVM _secondPlayer = null!;
    public MissionPeer? FirstPlayerPeer { get; private set; }
    public MissionPeer? SecondPlayerPeer { get; private set; }

    [DataSourceProperty]
    public bool IsEnabled
    {
        get
        {
            return _isEnabled;
        }
        set
        {
            if (value != _isEnabled)
            {
                _isEnabled = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsPreparing
    {
        get
        {
            return _isPreparing;
        }
        set
        {
            if (value != _isPreparing)
            {
                _isPreparing = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string CountdownMessage
    {
        get
        {
            return _countdownMessage;
        }
        set
        {
            if (value != _countdownMessage)
            {
                _countdownMessage = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string Score
    {
        get
        {
            return _score;
        }
        set
        {
            if (value != _score)
            {
                _score = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int FirstPlayerScore
    {
        get
        {
            return _firstPlayerScore;
        }
        set
        {
            if (value != _firstPlayerScore)
            {
                _firstPlayerScore = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int SecondPlayerScore
    {
        get
        {
            return _secondPlayerScore;
        }
        set
        {
            if (value != _secondPlayerScore)
            {
                _secondPlayerScore = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MPPlayerVM FirstPlayer
    {
        get
        {
            return _firstPlayer;
        }
        set
        {
            if (value != _firstPlayer)
            {
                _firstPlayer = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MPPlayerVM SecondPlayer
    {
        get
        {
            return _secondPlayer;
        }
        set
        {
            if (value != _secondPlayer)
            {
                _secondPlayer = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    public CrpgDuelMatchVm()
    {
        IsEnabled = false;
        _duelCountdownText = new TextObject("{=cO2FDHCa}Duel with {OPPONENT_NAME} is starting in {DUEL_REMAINING_TIME} seconds.");
        RefreshValues();
    }

    public void OnDuelPrepStarted(MissionPeer opponentPeer, int prepDuration)
    {
        _prepTimeRemaining = prepDuration;
        GameTexts.SetVariable("OPPONENT_NAME", opponentPeer.DisplayedName);
        IsPreparing = true;
    }

    public void Tick(float dt)
    {
        if (_prepTimeRemaining > 0f)
        {
            GameTexts.SetVariable("DUEL_REMAINING_TIME", (float)MathF.Ceiling(_prepTimeRemaining));
            CountdownMessage = _duelCountdownText.ToString();
            _prepTimeRemaining -= dt;
        }
        else
        {
            IsPreparing = false;
        }
    }

    public void OnDuelStarted(MissionPeer firstPeer, MissionPeer secondPeer)
    {
        FirstPlayerPeer = firstPeer;
        SecondPlayerPeer = secondPeer;
        FirstPlayerScore = 0;
        SecondPlayerScore = 0;
        FirstPlayer = new MPPlayerVM(firstPeer);
        SecondPlayer = new MPPlayerVM(secondPeer);
        FirstPlayer.RefreshDivision(useCultureColors: true);
        SecondPlayer.RefreshDivision(useCultureColors: true);
        IsEnabled = true;
    }

    public void OnDuelEnded()
    {
        FirstPlayerPeer = null;
        SecondPlayerPeer = null;
        IsEnabled = false;
    }

    public void OnPeerScored(MissionPeer peer)
    {
        if (peer == FirstPlayerPeer)
        {
            FirstPlayerScore++;
        }
        else if (peer == SecondPlayerPeer)
        {
            SecondPlayerScore++;
        }
    }

    public void RefreshNames(bool changeGenericNames = false)
    {
        if (changeGenericNames)
        {
            FirstPlayer.Name = FirstPlayerPeer?.DisplayedName;
            SecondPlayer.Name = SecondPlayerPeer?.DisplayedName;
        }
    }

    public override void RefreshValues()
    {
        Score = new TextObject("{=}vs.").ToString();
    }
}

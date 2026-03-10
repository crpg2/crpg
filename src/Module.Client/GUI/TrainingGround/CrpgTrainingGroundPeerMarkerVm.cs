using Crpg.Module.Common;
using Crpg.Module.Modes.TrainingGround;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

namespace Crpg.Module.GUI.TrainingGround;

public class CrpgTrainingGroundPeerMarkerVm : ViewModel
{
    private float _currentDuelRequestTimeRemaining;
    private float _latestX;
    private float _latestY;
    private float _latestW;
    private float _wPosAfterPositionCalculation;
    private TextObject _acceptDuelRequestText = new(string.Empty);
    private TextObject _sendDuelRequestText = new(string.Empty);
    private TextObject _waitingForDuelResponseText = new(string.Empty);
    private bool _isEnabled;
    private bool _isTracked;
    private bool _shouldShowInformation;
    private bool _isAgentInScreenBoundaries;
    private bool _isFocused;
    private bool _hasDuelRequestForPlayer;
    private bool _hasSentDuelRequest;
    private string _name = string.Empty;
    private string _actionDescriptionText = string.Empty;
    private string _clan = string.Empty;
    private string _clanText = string.Empty;
    private string _levelText = string.Empty;
    private int _level;
    private int _duelsWon;
    private int _duelsLost;
    private int _rating;
    private int _wSign;
    private Vec2 _screenPosition;
    private Vec3 _groundVec;
    private MPTeammateCompassTargetVM _compassElement = null!;
    public MissionPeer TargetPeer { get; private set; }
    public float Distance { get; private set; }
    public bool IsInDuel { get; private set; }

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
                UpdateTracked();
            }
        }
    }

    [DataSourceProperty]
    public bool IsTracked
    {
        get
        {
            return _isTracked;
        }
        set
        {
            if (value != _isTracked)
            {
                _isTracked = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool ShouldShowInformation
    {
        get
        {
            return _shouldShowInformation;
        }
        set
        {
            if (value != _shouldShowInformation)
            {
                _shouldShowInformation = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsAgentInScreenBoundaries
    {
        get
        {
            return _isAgentInScreenBoundaries;
        }
        set
        {
            if (value != _isAgentInScreenBoundaries)
            {
                _isAgentInScreenBoundaries = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsFocused
    {
        get
        {
            return _isFocused;
        }
        set
        {
            if (value != _isFocused)
            {
                _isFocused = value;
                OnPropertyChangedWithValue(value);
                SetFocused(value);
                UpdateTracked();
            }
        }
    }

    [DataSourceProperty]
    public bool HasDuelRequestForPlayer
    {
        get
        {
            return _hasDuelRequestForPlayer;
        }
        set
        {
            if (value != _hasDuelRequestForPlayer)
            {
                _hasDuelRequestForPlayer = value;
                OnPropertyChangedWithValue(value);
                OnInteractionChanged();
                UpdateTracked();
            }
        }
    }

    [DataSourceProperty]
    public bool HasSentDuelRequest
    {
        get
        {
            return _hasSentDuelRequest;
        }
        set
        {
            if (value != _hasSentDuelRequest)
            {
                _hasSentDuelRequest = value;
                OnPropertyChangedWithValue(value);
                OnInteractionChanged();
                UpdateTracked();
            }
        }
    }

    [DataSourceProperty]
    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            if (value != _name)
            {
                _name = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string ActionDescriptionText
    {
        get
        {
            return _actionDescriptionText;
        }
        set
        {
            if (value != _actionDescriptionText)
            {
                _actionDescriptionText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string Clan
    {
        get
        {
            return _clan;
        }
        set
        {
            if (value != _clan)
            {
                _clan = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string ClanText
    {
        get
        {
            return _clanText;
        }
        set
        {
            if (value != _clanText)
            {
                _clanText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int DuelsWon
    {
        get
        {
            return _duelsWon;
        }
        set
        {
            if (value != _duelsWon)
            {
                _duelsWon = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int DuelsLost
    {
        get
        {
            return _duelsLost;
        }
        set
        {
            if (value != _duelsLost)
            {
                _duelsLost = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int Rating
    {
        get
        {
            return _rating;
        }
        set
        {
            if (value != _rating)
            {
                _rating = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int Level
    {
        get
        {
            return _level;
        }
        set
        {
            if (value != _level)
            {
                _level = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string LevelText
    {
        get
        {
            return _levelText;
        }
        set
        {
            if (value != _levelText)
            {
                _levelText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public int WSign
    {
        get
        {
            return _wSign;
        }
        set
        {
            if (value != _wSign)
            {
                _wSign = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public Vec2 ScreenPosition
    {
        get
        {
            return _screenPosition;
        }
        set
        {
            if (value.x != _screenPosition.x || value.y != _screenPosition.y)
            {
                _screenPosition = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public Vec3 GroundVec
    {
        get
        {
            return _groundVec;
        }
        set
        {
            if (value.x != _groundVec.x || value.y != _groundVec.y || value.z != _groundVec.z)
            {
                _groundVec = value;
            }
        }
    }

    [DataSourceProperty]
    public MPTeammateCompassTargetVM CompassElement
    {
        get
        {
            return _compassElement;
        }
        set
        {
            if (value != _compassElement)
            {
                _compassElement = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    public CrpgTrainingGroundPeerMarkerVm(MissionPeer peer)
    {
        TargetPeer = peer;
        var crpgPeer = peer.GetComponent<CrpgPeer>();
        int crpgLevel = crpgPeer.User?.Character.Level ?? 0;
        string? crpgClan = crpgPeer.Clan?.Name;
        Level = crpgLevel;
        Clan = crpgClan ?? new TextObject("{=koX9okuG}None").ToString();
        ClanText = new TextObject("{=}Clan: ").ToString();
        LevelText = new TextObject("{=}Level: ").ToString();
        Rating = ((CrpgTrainingGroundMissionRepresentative)peer.Representative).Rating;
        DuelsWon = ((CrpgTrainingGroundMissionRepresentative)peer.Representative).NumberOfWins;
        DuelsLost = ((CrpgTrainingGroundMissionRepresentative)peer.Representative).NumberOfLosses;
        IsEnabled = true;
        TargetIconType iconType = MultiplayerClassDivisions.GetMPHeroClassForPeer(TargetPeer).IconType;
        CompassElement = new MPTeammateCompassTargetVM(iconType, Color.White.ToUnsignedInteger(), Color.White.ToUnsignedInteger(), new Banner(), isAlly: true);
        RefreshValues();
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        Name = TargetPeer.DisplayedName;
        _acceptDuelRequestText = new TextObject("{=tidE1V1k}Accept duel");
        _sendDuelRequestText = new TextObject("{=YLPJWgqF}Challenge");
        _waitingForDuelResponseText = new TextObject("{=MPgnsZoo}Waiting for response");
    }

    public void OnTick(float dt)
    {
        if (Agent.Main != null && TargetPeer.ControlledAgent != null)
        {
            Distance = _latestW;
        }

        if (HasSentDuelRequest)
        {
            _currentDuelRequestTimeRemaining -= dt;
            GameTexts.SetVariable("SECONDS", (int)_currentDuelRequestTimeRemaining);
            GameTexts.SetVariable("ACTION", _waitingForDuelResponseText);
            ActionDescriptionText = new TextObject("{=HXWpxvgT}{ACTION} ({SECONDS})").ToString();
            if (_currentDuelRequestTimeRemaining <= 0f)
            {
                HasSentDuelRequest = false;
            }
        }
    }

    public void UpdateScreenPosition(Camera missionCamera)
    {
        if (TargetPeer.ControlledAgent != null)
        {
            _groundVec = TargetPeer.ControlledAgent.GetWorldPosition().GetGroundVec3();
            _groundVec += new Vec3(0f, 0f, TargetPeer.ControlledAgent.GetEyeGlobalHeight());
            _latestX = 0f;
            _latestY = 0f;
            _latestW = 0f;
            MBWindowManager.WorldToScreen(missionCamera, _groundVec, ref _latestX, ref _latestY, ref _latestW);
            GroundVec = _groundVec;
            ScreenPosition = new Vec2(_latestX, _latestY);
            IsAgentInScreenBoundaries = !(_latestX > Screen.RealScreenResolutionWidth) && !(_latestY > Screen.RealScreenResolutionHeight) && !(_latestX + 200f < 0f) && !(_latestY + 100f < 0f);
            _wPosAfterPositionCalculation = _latestW < 0f ? -1f : 1.1f;
            WSign = (int)_wPosAfterPositionCalculation;
        }
    }

    public void UpdateRecord()
    {
        DuelsWon = ((CrpgTrainingGroundMissionRepresentative)TargetPeer.Representative).NumberOfWins;
        DuelsLost = ((CrpgTrainingGroundMissionRepresentative)TargetPeer.Representative).NumberOfLosses;
    }

    public void UpdateRating()
    {
        Rating = ((CrpgTrainingGroundMissionRepresentative)TargetPeer.Representative).Rating;
    }

    public void OnDuelStarted()
    {
        IsEnabled = false;
        IsInDuel = true;
    }

    public void OnDuelEnded()
    {
        IsEnabled = true;
        IsInDuel = false;
    }

    public void UpdateCurentDuelStatus(bool isInDuel)
    {
        IsInDuel = isInDuel;
        IsEnabled = !IsInDuel;
    }

    private void OnInteractionChanged()
    {
        ActionDescriptionText = string.Empty;
        if (HasDuelRequestForPlayer)
        {
            string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13));
            GameTexts.SetVariable("KEY", keyHyperlinkText);
            GameTexts.SetVariable("ACTION", _acceptDuelRequestText);
            ActionDescriptionText = GameTexts.FindText("str_key_action").ToString();
        }
        else if (HasSentDuelRequest)
        {
            _currentDuelRequestTimeRemaining = 10f;
        }
    }

    private void SetFocused(bool isFocused)
    {
        if (!HasDuelRequestForPlayer && !HasSentDuelRequest)
        {
            if (isFocused)
            {
                string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13));
                GameTexts.SetVariable("KEY", keyHyperlinkText);
                GameTexts.SetVariable("ACTION", _sendDuelRequestText);
                ActionDescriptionText = GameTexts.FindText("str_key_action").ToString();
            }
            else
            {
                ActionDescriptionText = string.Empty;
            }
        }
    }

    private void UpdateTracked()
    {
        if (!IsEnabled)
        {
            IsTracked = false;
        }
        else if (HasDuelRequestForPlayer || HasSentDuelRequest || IsFocused)
        {
            IsTracked = true;
        }
        else
        {
            IsTracked = false;
        }

        ShouldShowInformation = IsTracked || IsFocused;
    }
}

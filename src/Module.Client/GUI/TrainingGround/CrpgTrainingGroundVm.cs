﻿using Crpg.Module.Modes.TrainingGround;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.HUDExtensions;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.KillFeed;

namespace Crpg.Module.GUI.TrainingGround;
internal class CrpgTrainingGroundVm : ViewModel
{
    private readonly CrpgTrainingGroundMissionMultiplayerClient _client;
    private readonly MissionMultiplayerGameModeBaseClient _gameMode;
    private bool _isMyRepresentativeAssigned;
    private bool _isAgentBuiltForTheFirstTime = true;
    private string _cachedPlayerClassID = string.Empty;
    private Camera _missionCamera;
    private bool _isEnabled;
    private bool _areOngoingDuelsActive;
    private bool _isPlayerInDuel;
    private string _playerScoreText = string.Empty;
    private string _remainingRoundTime = string.Empty;
    private CrpgTrainingGroundMarkersVm _markers = default!;
    private CrpgDuelMatchVm _playerDuelMatch = default!;
    private MBBindingList<CrpgDuelMatchVm> _ongoingDuels = default!;
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
                OnPropertyChangedWithValue(value, "IsEnabled");
            }
        }
    }

    [DataSourceProperty]
    public bool AreOngoingDuelsActive
    {
        get
        {
            return _areOngoingDuelsActive;
        }
        set
        {
            if (value != _areOngoingDuelsActive)
            {
                _areOngoingDuelsActive = value;
                OnPropertyChangedWithValue(value, "AreOngoingDuelsActive");
            }
        }
    }

    [DataSourceProperty]
    public bool IsPlayerInDuel
    {
        get
        {
            return _isPlayerInDuel;
        }
        set
        {
            if (value != _isPlayerInDuel)
            {
                _isPlayerInDuel = value;
                OnPropertyChangedWithValue(value, "IsPlayerInDuel");
            }
        }
    }

    [DataSourceProperty]
    public string PlayerScoreText
    {
        get
        {
            return _playerScoreText;
        }
        set
        {
            if (value != _playerScoreText)
            {
                _playerScoreText = value;
                OnPropertyChangedWithValue(value, "PlayerScoreText");
            }
        }
    }

    [DataSourceProperty]
    public string RemainingRoundTime
    {
        get
        {
            return _remainingRoundTime;
        }
        set
        {
            if (value != _remainingRoundTime)
            {
                _remainingRoundTime = value;
                OnPropertyChangedWithValue(value, "RemainingRoundTime");
            }
        }
    }

    [DataSourceProperty]
    public CrpgTrainingGroundMarkersVm Markers
    {
        get
        {
            return _markers;
        }
        set
        {
            if (value != _markers)
            {
                _markers = value;
                OnPropertyChangedWithValue(value, "Markers");
            }
        }
    }

    [DataSourceProperty]
    public CrpgDuelMatchVm PlayerDuelMatch
    {
        get
        {
            return _playerDuelMatch;
        }
        set
        {
            if (value != _playerDuelMatch)
            {
                _playerDuelMatch = value;
                OnPropertyChangedWithValue(value, "PlayerDuelMatch");
            }
        }
    }

    [DataSourceProperty]
    public MBBindingList<CrpgDuelMatchVm> OngoingDuels
    {
        get
        {
            return _ongoingDuels;
        }
        set
        {
            if (value != _ongoingDuels)
            {
                _ongoingDuels = value;
                OnPropertyChangedWithValue(value, "OngoingDuels");
            }
        }
    }

    public CrpgTrainingGroundVm(Camera missionCamera, CrpgTrainingGroundMissionMultiplayerClient client)
    {
        _missionCamera = missionCamera;
        _client = client;
        CrpgTrainingGroundMissionMultiplayerClient client2 = _client;
        client2.OnMyRepresentativeAssigned = (Action)Delegate.Combine(client2.OnMyRepresentativeAssigned, new Action(OnMyRepresentativeAssigned));
        _gameMode = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        PlayerDuelMatch = new CrpgDuelMatchVm();
        OngoingDuels = new MBBindingList<CrpgDuelMatchVm>();
        Markers = new CrpgTrainingGroundMarkersVm(missionCamera, _client);
        RefreshValues();
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        PlayerDuelMatch.RefreshValues();
        Markers.RefreshValues();
    }

    private void OnMyRepresentativeAssigned()
    {
        CrpgTrainingGroundMissionRepresentative myRepresentative = _client.MyRepresentative;
        myRepresentative.OnDuelPrepStartedEvent += OnDuelPrepStarted;
        myRepresentative.OnAgentSpawnedWithoutDuelEvent += OnAgentSpawnedWithoutDuel;
        myRepresentative.OnDuelPreparationStartedForTheFirstTimeEvent += OnDuelStarted;
        myRepresentative.OnDuelEndedEvent += OnDuelEnded;
        myRepresentative.OnDuelRoundEndedEvent += OnDuelRoundEnded;
        myRepresentative.OnDuelResult += OnDuelResult;
        ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
        Markers.RegisterEvents();
        _isMyRepresentativeAssigned = true;
    }

    public void Tick(float dt)
    {
        if (_gameMode.CheckTimer(out int remainingTime, out int _))
        {
            RemainingRoundTime = TimeSpan.FromSeconds(remainingTime).ToString("mm':'ss");
        }

        Markers.Tick(dt);
        if (PlayerDuelMatch.IsEnabled)
        {
            PlayerDuelMatch.Tick(dt);
        }
    }

    public override void OnFinalize()
    {
        base.OnFinalize();
        CrpgTrainingGroundMissionMultiplayerClient client = _client;
        client.OnMyRepresentativeAssigned = (Action)Delegate.Remove(client.OnMyRepresentativeAssigned, new Action(OnMyRepresentativeAssigned));
        if (_isMyRepresentativeAssigned)
        {
            CrpgTrainingGroundMissionRepresentative myRepresentative = _client.MyRepresentative;
            myRepresentative.OnDuelPrepStartedEvent -= OnDuelPrepStarted;
            myRepresentative.OnAgentSpawnedWithoutDuelEvent -= OnAgentSpawnedWithoutDuel;
            myRepresentative.OnDuelPreparationStartedForTheFirstTimeEvent -= OnDuelStarted;
            myRepresentative.OnDuelEndedEvent -= OnDuelEnded;
            myRepresentative.OnDuelRoundEndedEvent -= OnDuelRoundEnded;
            myRepresentative.OnDuelResult -= OnDuelResult;
            ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Remove(ManagedOptions.OnManagedOptionChanged, new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionChanged));
            Markers.UnregisterEvents();
        }
    }

    private void OnManagedOptionChanged(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
    {
        if (changedManagedOptionsType == ManagedOptions.ManagedOptionsType.EnableGenericNames)
        {
            _ongoingDuels.ApplyActionOnAllItems(delegate(CrpgDuelMatchVm d)
            {
                d.RefreshNames(changeGenericNames: true);
            });
        }
    }

    private void OnDuelPrepStarted(MissionPeer opponentPeer, int duelStartTime)
    {
        PlayerDuelMatch.OnDuelPrepStarted(opponentPeer, duelStartTime);
        AreOngoingDuelsActive = false;
        Markers.IsEnabled = false;
    }

    private void OnAgentSpawnedWithoutDuel()
    {
        Markers.OnAgentSpawnedWithoutDuel();
        AreOngoingDuelsActive = true;
    }

    private void OnDuelStarted(MissionPeer firstPeer, MissionPeer secondPeer)
    {
        Markers.OnDuelStarted(firstPeer, secondPeer);
        if (firstPeer == _client.MyRepresentative.MissionPeer || secondPeer == _client.MyRepresentative.MissionPeer)
        {
            AreOngoingDuelsActive = false;
            IsPlayerInDuel = true;
            PlayerDuelMatch.OnDuelStarted(firstPeer, secondPeer);
        }
        else
        {
            CrpgDuelMatchVm duelMatchVM = new();
            duelMatchVM.OnDuelStarted(firstPeer, secondPeer);
            OngoingDuels.Add(duelMatchVM);
        }
    }

    private void OnDuelEnded(MissionPeer winnerPeer)
    {
        if (PlayerDuelMatch.FirstPlayerPeer == winnerPeer || PlayerDuelMatch.SecondPlayerPeer == winnerPeer)
        {
            AreOngoingDuelsActive = true;
            IsPlayerInDuel = false;
            Markers.IsEnabled = true;
            Markers.SetMarkerOfPeerEnabled(PlayerDuelMatch.FirstPlayerPeer!, isEnabled: true);
            Markers.SetMarkerOfPeerEnabled(PlayerDuelMatch.SecondPlayerPeer!, isEnabled: true);
            PlayerDuelMatch.OnDuelEnded();
        }

        CrpgDuelMatchVm duelMatchVM = OngoingDuels.FirstOrDefault((CrpgDuelMatchVm d) => d.FirstPlayerPeer == winnerPeer || d.SecondPlayerPeer == winnerPeer);
        if (duelMatchVM != null)
        {
            Markers.SetMarkerOfPeerEnabled(duelMatchVM.FirstPlayerPeer!, isEnabled: true);
            Markers.SetMarkerOfPeerEnabled(duelMatchVM.SecondPlayerPeer!, isEnabled: true);
            OngoingDuels.Remove(duelMatchVM);
        }
    }

    private void OnDuelRoundEnded(MissionPeer winnerPeer)
    {
        if (PlayerDuelMatch.FirstPlayerPeer == winnerPeer || PlayerDuelMatch.SecondPlayerPeer == winnerPeer)
        {
            PlayerDuelMatch.OnPeerScored(winnerPeer);
            return;
        }

        CrpgDuelMatchVm duelMatchVM = OngoingDuels.FirstOrDefault((CrpgDuelMatchVm d) => d.FirstPlayerPeer == winnerPeer || d.SecondPlayerPeer == winnerPeer);
        if (duelMatchVM != null)
        {
            duelMatchVM.OnPeerScored(winnerPeer);
            }
    }

    private void OnDuelResult(bool hasWonDuel, int ratingChange)
    {

    }

    public void OnScreenResolutionChanged()
    {
        Markers.UpdateScreenCenter();
    }

    public void OnMainAgentRemoved()
    {
        if (!PlayerDuelMatch.IsEnabled)
        {
            Markers.IsEnabled = false;
            AreOngoingDuelsActive = false;
        }
    }

    public void OnMainAgentBuild()
    {
        if (!PlayerDuelMatch.IsEnabled)
        {
            Markers.IsEnabled = true;
            AreOngoingDuelsActive = true;
        }

        string stringId = MultiplayerClassDivisions.GetMPHeroClassForPeer(_client.MyRepresentative.MissionPeer).StringId;
        if (_isAgentBuiltForTheFirstTime || (stringId != _cachedPlayerClassID))
        {
            _isAgentBuiltForTheFirstTime = false;
            _cachedPlayerClassID = stringId;
        }
    }

}

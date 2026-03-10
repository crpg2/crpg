using Crpg.Module.Modes.TrainingGround;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.TrainingGround;

public class CrpgTrainingGroundMarkersVm : ViewModel
{
    private class PeerMarkerDistanceComparer : IComparer<CrpgTrainingGroundPeerMarkerVm>
    {
        public int Compare(CrpgTrainingGroundPeerMarkerVm x, CrpgTrainingGroundPeerMarkerVm y)
        {
            return y.Distance.CompareTo(x.Distance);
        }
    }

    private const float FocusScreenDistanceThreshold = 250f;
    private const float FocusAgentDistanceThreshold = 6f;
    private readonly Camera _missionCamera;
    private readonly PeerMarkerDistanceComparer _distanceComparer;
    private readonly Dictionary<MissionPeer, CrpgTrainingGroundPeerMarkerVm> _targetPeersToMarkersDictionary;
    private readonly CrpgTrainingGroundMissionMultiplayerClient _client;
    private readonly Dictionary<MissionPeer, bool> _targetPeersInDuelDictionary;
    private bool _hasEnteredLobby;
    private CrpgTrainingGroundPeerMarkerVm? _previousFocusTarget;
    private CrpgTrainingGroundPeerMarkerVm? _currentFocusTarget;
    private Vec2 _screenCenter;
    private bool _isPlayerFocused;
    private bool _isEnabled;
    private MBBindingList<CrpgTrainingGroundPeerMarkerVm> _targets = null!;

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
                UpdateTargetsEnabled(value);
            }
        }
    }

    [DataSourceProperty]
    public MBBindingList<CrpgTrainingGroundPeerMarkerVm> Targets
    {
        get
        {
            return _targets;
        }
        set
        {
            if (value != _targets)
            {
                _targets = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    public CrpgTrainingGroundMarkersVm(Camera missionCamera, CrpgTrainingGroundMissionMultiplayerClient client)
    {
        _missionCamera = missionCamera;
        _client = client;

        Targets = new MBBindingList<CrpgTrainingGroundPeerMarkerVm>();
        _targetPeersToMarkersDictionary = new Dictionary<MissionPeer, CrpgTrainingGroundPeerMarkerVm>();
        _targetPeersInDuelDictionary = new Dictionary<MissionPeer, bool>();
        _distanceComparer = new PeerMarkerDistanceComparer();
        UpdateScreenCenter();
        RefreshValues();
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        Targets.ApplyActionOnAllItems(t =>
        {
            t.RefreshValues();
        });
    }

    public void UpdateScreenCenter()
    {
        _screenCenter = new Vec2(Screen.RealScreenResolutionWidth / 2f, Screen.RealScreenResolutionHeight / 2f);
    }

    public void Tick(float dt)
    {
        if (_hasEnteredLobby && GameNetwork.MyPeer != null)
        {
            OnRefreshPeerMarkers();
            UpdateTargets(dt);
        }
    }

    public void RegisterEvents()
    {
        CrpgTrainingGroundMissionRepresentative myRepresentative = _client.MyRepresentative;
        myRepresentative.OnDuelRequestSentEvent += OnDuelRequestSent;
        myRepresentative.OnDuelRequestedEvent += OnDuelRequested;
        ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate)Delegate.Combine(
            ManagedOptions.OnManagedOptionChanged,
            new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionsChanged));
    }

    public void UnregisterEvents()
    {
        CrpgTrainingGroundMissionRepresentative myRepresentative = _client.MyRepresentative;
        myRepresentative.OnDuelRequestSentEvent -= OnDuelRequestSent;
        myRepresentative.OnDuelRequestedEvent -= OnDuelRequested;
        ManagedOptions.OnManagedOptionChanged = (ManagedOptions.OnManagedOptionChangedDelegate?)Delegate.Remove(
            ManagedOptions.OnManagedOptionChanged,
            new ManagedOptions.OnManagedOptionChangedDelegate(OnManagedOptionsChanged));
    }

    public void RefreshPeerEquipments()
    {
        foreach (MissionPeer item in VirtualPlayer.Peers<MissionPeer>())
        {
            OnPeerEquipmentRefreshed(item);
        }
    }

    public void OnAgentSpawnedWithoutDuel()
    {
        _hasEnteredLobby = true;
        IsEnabled = true;
    }

    public void OnDuelStarted(MissionPeer firstPeer, MissionPeer secondPeer)
    {
        if (_client.MyRepresentative.MissionPeer == firstPeer || _client.MyRepresentative.MissionPeer == secondPeer)
        {
            IsEnabled = false;
        }

        foreach (CrpgTrainingGroundPeerMarkerVm target in Targets)
        {
            if (target.TargetPeer == firstPeer || target.TargetPeer == secondPeer)
            {
                target.OnDuelStarted();
            }
        }

        _targetPeersInDuelDictionary[firstPeer] = true;
        _targetPeersInDuelDictionary[secondPeer] = true;
    }

    public void SetMarkerOfPeerEnabled(MissionPeer peer, bool isEnabled)
    {
        if (_targetPeersToMarkersDictionary.ContainsKey(peer))
        {
            _targetPeersToMarkersDictionary[peer].UpdateCurentDuelStatus(!isEnabled);
            _targetPeersToMarkersDictionary[peer].UpdateRecord();
            _targetPeersToMarkersDictionary[peer].UpdateRating();
        }

        if (_targetPeersInDuelDictionary.ContainsKey(peer))
        {
            _targetPeersInDuelDictionary[peer] = !isEnabled;
        }
    }

    public void OnFocusGained()
    {
        _isPlayerFocused = true;
    }

    public void OnFocusLost()
    {
        _isPlayerFocused = false;
    }

    public void OnPeerEquipmentRefreshed(MissionPeer peer)
    {
        if (_targetPeersToMarkersDictionary.TryGetValue(peer, out CrpgTrainingGroundPeerMarkerVm? _))
        {
        }
    }

    private void OnManagedOptionsChanged(ManagedOptions.ManagedOptionsType changedManagedOptionsType)
    {
        if (changedManagedOptionsType == ManagedOptions.ManagedOptionsType.EnableGenericNames)
        {
            Targets.ApplyActionOnAllItems(t =>
            {
                t.RefreshValues();
            });
        }
    }

    private void UpdateTargets(float dt)
    {
        if (_currentFocusTarget != null)
        {
            _previousFocusTarget = _currentFocusTarget;
            _currentFocusTarget = null;
            if (_isPlayerFocused)
            {
                _previousFocusTarget.IsFocused = false;
            }
        }

        if (_client.MyRepresentative.MissionPeer.ControlledAgent == null)
        {
            return;
        }

        float num = float.MaxValue;
        foreach (CrpgTrainingGroundPeerMarkerVm target in Targets)
        {
            target.OnTick(dt);
            if (target.IsEnabled)
            {
                target.UpdateScreenPosition(_missionCamera);
                target.HasDuelRequestForPlayer = _client.MyRepresentative.CheckHasRequestFromAndRemoveRequestIfNeeded(target.TargetPeer);
                float num2 = target.ScreenPosition.Distance(_screenCenter);
                float num3 = target.GroundVec.Distance(_client.MyRepresentative.MissionPeer.ControlledAgent.Position);
                if (!_isPlayerFocused && target.WSign >= 0 && num2 < FocusScreenDistanceThreshold && num2 < num && num3 < FocusAgentDistanceThreshold)
                {
                    num = num2;
                    _currentFocusTarget = target;
                }
            }
        }

        Targets.Sort(_distanceComparer);

        if (_currentFocusTarget != null && _currentFocusTarget.TargetPeer.ControlledAgent != null)
        {
            _client.MyRepresentative.OnObjectFocused(_currentFocusTarget!.TargetPeer.ControlledAgent);
            if (_previousFocusTarget != null && _currentFocusTarget.TargetPeer != _previousFocusTarget.TargetPeer)
            {
                _previousFocusTarget!.IsFocused = false;
            }

            _currentFocusTarget.IsFocused = true;

            return;
        }

        if (_previousFocusTarget != null)
        {
            _previousFocusTarget.IsFocused = false;
        }

        if (_currentFocusTarget == null)
        {
            _client.MyRepresentative.OnObjectFocusLost();
        }
    }

    private void OnRefreshPeerMarkers()
    {
        List<CrpgTrainingGroundPeerMarkerVm> list = Targets.ToList();
        foreach (MissionPeer item in VirtualPlayer.Peers<MissionPeer>())
        {
            if (item?.Team == null || !item.IsControlledAgentActive || item.IsMine)
            {
                continue;
            }

            if (!_targetPeersToMarkersDictionary.ContainsKey(item))
            {
                CrpgTrainingGroundPeerMarkerVm crpgTrainingGroundPeerMarkerVm = new(item);
                Targets.Add(crpgTrainingGroundPeerMarkerVm);
                _targetPeersToMarkersDictionary.Add(item, crpgTrainingGroundPeerMarkerVm);
                OnPeerEquipmentRefreshed(item);
                if (_targetPeersInDuelDictionary.ContainsKey(item))
                {
                    crpgTrainingGroundPeerMarkerVm.UpdateCurentDuelStatus(_targetPeersInDuelDictionary[item]);
                }
            }
            else
            {
                list.Remove(_targetPeersToMarkersDictionary[item]);
            }

            if (!_targetPeersInDuelDictionary.ContainsKey(item))
            {
                _targetPeersInDuelDictionary.Add(item, value: false);
            }
        }

        foreach (CrpgTrainingGroundPeerMarkerVm item2 in list)
        {
            Targets.Remove(item2);
            _targetPeersToMarkersDictionary.Remove(item2.TargetPeer);
        }
    }

    private void UpdateTargetsEnabled(bool isEnabled)
    {
        foreach (CrpgTrainingGroundPeerMarkerVm target in Targets)
        {
            target.IsEnabled = !target.IsInDuel && isEnabled;
        }
    }

    private void OnDuelRequestSent(MissionPeer targetPeer)
    {
        foreach (CrpgTrainingGroundPeerMarkerVm target in Targets)
        {
            if (target.TargetPeer == targetPeer)
            {
                target.HasSentDuelRequest = true;
            }
        }
    }

    private void OnDuelRequested(MissionPeer targetPeer)
    {
        var crpgTrainingGroundPeerMarkerVm = Targets.FirstOrDefault(t => t.TargetPeer == targetPeer);
        if (crpgTrainingGroundPeerMarkerVm != null)
        {
            crpgTrainingGroundPeerMarkerVm.HasDuelRequestForPlayer = true;
        }
    }
}

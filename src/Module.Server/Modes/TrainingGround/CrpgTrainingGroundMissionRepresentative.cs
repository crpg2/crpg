using NetworkMessages.FromClient;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.TrainingGround;

public class CrpgTrainingGroundMissionRepresentative : MissionRepresentativeBase
{
    public const int DuelPrepTime = 3;
    public event Action<MissionPeer>? OnDuelRequestedEvent;
    public event Action<bool, int>? OnDuelResult;
    public event Action<MissionPeer>? OnDuelRequestSentEvent;
    public event Action<MissionPeer, int>? OnDuelPrepStartedEvent;
    public event Action? OnAgentSpawnedWithoutDuelEvent;
    public event Action<MissionPeer, MissionPeer>? OnDuelPreparationStartedForTheFirstTimeEvent;
    public event Action<MissionPeer>? OnDuelEndedEvent;
    public event Action<MissionPeer>? OnDuelRoundEndedEvent;
    private List<Tuple<MissionPeer, MissionTime>> _requesters = null!;
    private IFocusable? _focusedObject;
#if CRPG_SERVER
    private CrpgTrainingGroundServer _mission = null!;
#endif

    public bool HasLoadedStats { get; set; }
    public int NumberOfWins { get; set; }
    public int NumberOfLosses { get; set; }
    public int Rating { get; set; }

    public override void Initialize()
    {
        _requesters = new List<Tuple<MissionPeer, MissionTime>>();
#if CRPG_SERVER
        if (GameNetwork.IsServerOrRecorder)
        {
            _mission = Mission.Current.GetMissionBehavior<CrpgTrainingGroundServer>();
        }
#endif
        Mission.Current.SetMissionMode(MissionMode.Duel, atStart: true);
    }

    public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
    {
        if (GameNetwork.IsClient)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new(mode);
            networkMessageHandlerRegisterer.Register<TrainingGroundServerDuelRequest>(HandleServerEventDuelRequest);
            networkMessageHandlerRegisterer.Register<DuelSessionStarted>(HandleServerEventDuelSessionStarted);
            networkMessageHandlerRegisterer.Register<TrainingGroundDuelPreparationStartedForTheFirstTime>(HandleServerEventDuelStarted);
            networkMessageHandlerRegisterer.Register<DuelEnded>(HandleServerEventDuelEnded);
            networkMessageHandlerRegisterer.Register<DuelRoundEnded>(HandleServerEventDuelRoundEnded);
            networkMessageHandlerRegisterer.Register<TrainingGroundDuelPointsUpdateMessage>(HandleServerPointUpdate);
            networkMessageHandlerRegisterer.Register<TrainingGroundDuelResultMessage>(HandleServerEventDuelResult);
        }
    }

    public void OnInteraction(bool isModifiedInteraction = false)
    {
        if (_focusedObject == null)
        {
            return;
        }

        IFocusable focusedObject = _focusedObject;
        if (focusedObject is Agent focusedAgent)
        {
            if (!focusedAgent.IsActive())
            {
                return;
            }

            if (_requesters.Any(req => req.Item1 == focusedAgent.MissionPeer))
            {
                for (int i = 0; i < _requesters.Count; i++)
                {
                    if (_requesters[i].Item1 == MissionPeer)
                    {
                        _requesters.Remove(_requesters[i]);
                        break;
                    }
                }

                switch (PlayerType)
                {
                    case PlayerTypes.Client:
                        GameNetwork.BeginModuleEventAsClient();
                        GameNetwork.WriteMessage(new DuelResponse(focusedAgent.MissionRepresentative.Peer.Communicator as NetworkCommunicator, accepted: true));
                        GameNetwork.EndModuleEventAsClient();
                        break;
#if CRPG_SERVER
                    case PlayerTypes.Server:
                        _mission.DuelRequestAccepted(focusedAgent, ControlledAgent);
                        break;
#endif
                }
            }
            else
            {
                switch (PlayerType)
                {
                    case PlayerTypes.Client:
                        OnDuelRequestSentEvent?.Invoke(focusedAgent.MissionPeer);
                        GameNetwork.BeginModuleEventAsClient();
                        GameNetwork.WriteMessage(new TrainingGroundClientDuelRequest { RequestedAgentIndex = focusedAgent.Index });
                        GameNetwork.EndModuleEventAsClient();
                        break;
#if CRPG_SERVER
                    case PlayerTypes.Server:
                        _mission.DuelRequestReceived(MissionPeer, focusedAgent.MissionPeer);
                        break;
#endif
                }
            }
        }
    }

    public void DuelRequested(Agent requesterAgent)
    {
        _requesters.Add(new Tuple<MissionPeer, MissionTime>(requesterAgent.MissionPeer, MissionTime.Now + MissionTime.Seconds(10f)));
        switch (PlayerType)
        {
#if CRPG_SERVER
            case PlayerTypes.Bot:
                _mission.DuelRequestAccepted(requesterAgent, ControlledAgent);
                break;
            case PlayerTypes.Server:
                OnDuelRequestedEvent?.Invoke(requesterAgent.MissionPeer);
                break;
#endif
            case PlayerTypes.Client:
                if (IsMine)
                {
                    OnDuelRequestedEvent?.Invoke(requesterAgent.MissionPeer);
                    break;
                }

                GameNetwork.BeginModuleEventAsServer(Peer);
                GameNetwork.WriteMessage(new TrainingGroundServerDuelRequest { RequesterAgentIndex = requesterAgent.Index, RequestedAgentIndex = ControlledAgent.Index });
                GameNetwork.EndModuleEventAsServer();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool CheckHasRequestFromAndRemoveRequestIfNeeded(MissionPeer? requestOwner)
    {
        if (requestOwner != null && requestOwner.Representative == this)
        {
            _requesters.Clear();
            return false;
        }

        Tuple<MissionPeer, MissionTime>? tuple = _requesters.FirstOrDefault(req => req.Item1 == requestOwner);
        if (tuple == null)
        {
            return false;
        }

        if (requestOwner?.ControlledAgent == null || !requestOwner.ControlledAgent.IsActive())
        {
            _requesters.Remove(tuple);
            return false;
        }

        if (!tuple.Item2.IsPast)
        {
            return true;
        }

        _requesters.Remove(tuple);
        return false;
    }

    public void OnDuelPreparation(MissionPeer requesterPeer, MissionPeer requesteePeer)
    {
        switch (PlayerType)
        {
            case PlayerTypes.Client:
                if (IsMine)
                {
                    OnDuelPrepStartedEvent?.Invoke(MissionPeer == requesterPeer ? requesteePeer : requesterPeer, 5);
                    break;
                }

                GameNetwork.BeginModuleEventAsServer(Peer);
                GameNetwork.WriteMessage(new DuelSessionStarted(requesterPeer.GetNetworkPeer(), requesteePeer.GetNetworkPeer()));
                GameNetwork.EndModuleEventAsServer();
                break;
            case PlayerTypes.Server:
                OnDuelPrepStartedEvent?.Invoke(MissionPeer == requesterPeer ? requesteePeer : requesterPeer, 5);
                break;
        }

        Tuple<MissionPeer, MissionTime>? tuple = _requesters.FirstOrDefault(req => req.Item1 == requesterPeer);
        if (tuple != null)
        {
            _requesters.Remove(tuple);
        }
    }

    public void OnObjectFocused(IFocusable focusedObject)
    {
        _focusedObject = focusedObject;
    }

    public void OnObjectFocusLost()
    {
        _focusedObject = null;
    }

    public override void OnAgentSpawned()
    {
        if (ControlledAgent.Team != null && ControlledAgent.Team.Side == BattleSideEnum.Attacker)
        {
            OnAgentSpawnedWithoutDuelEvent?.Invoke();
        }
    }

    public void OnDuelWon()
    {
        NumberOfWins++;
    }

    public void OnDuelLost()
    {
        NumberOfLosses++;
    }

    private void HandleServerEventDuelRequest(TrainingGroundServerDuelRequest message)
    {
        Agent agentFromIndex = Mission.MissionNetworkHelper.GetAgentFromIndex(message.RequesterAgentIndex);
        Mission.MissionNetworkHelper.GetAgentFromIndex(message.RequestedAgentIndex);
        DuelRequested(agentFromIndex);
    }

    private void HandleServerEventDuelSessionStarted(DuelSessionStarted message)
    {
        OnDuelPreparation(message.RequesterPeer.GetComponent<MissionPeer>(), message.RequestedPeer.GetComponent<MissionPeer>());
    }

    private void HandleServerEventDuelStarted(TrainingGroundDuelPreparationStartedForTheFirstTime message)
    {
        MissionPeer component = message.RequesterPeer.GetComponent<MissionPeer>();
        MissionPeer component2 = message.RequesteePeer.GetComponent<MissionPeer>();
        OnDuelPreparationStartedForTheFirstTimeEvent?.Invoke(component, component2);
    }

    private void HandleServerEventDuelEnded(DuelEnded message)
    {
        OnDuelEndedEvent?.Invoke(message.WinnerPeer.GetComponent<MissionPeer>());
    }

    private void HandleServerEventDuelRoundEnded(DuelRoundEnded message)
    {
        OnDuelRoundEndedEvent?.Invoke(message.WinnerPeer.GetComponent<MissionPeer>());
    }

    private void HandleServerPointUpdate(TrainingGroundDuelPointsUpdateMessage message)
    {
        CrpgTrainingGroundMissionRepresentative component = message.NetworkCommunicator.GetComponent<CrpgTrainingGroundMissionRepresentative>();
        component.NumberOfLosses = message.NumberOfLosses;
        component.NumberOfWins = message.NumberOfWins;
        component.Rating = message.Rating;
    }

    private void HandleServerEventDuelResult(TrainingGroundDuelResultMessage message)
    {
        TextObject textObject = new("{=}You {RESULT} the duel! Your rating is now: {RATINGCHANGE}",
        new Dictionary<string, object>
        {
            ["RESULT"] = message.HasWonDuel ? new TextObject("{=}won").ToString() : new TextObject("{=}lost").ToString(),
            ["RATINGCHANGE"] = message.RatingChange.ToString(),
        });
        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = textObject.ToString(),
            Color = message.HasWonDuel ? new(0.45f, 0.86f, 0.45f) : new(0.96f, 0.58f, 0.47f),
            SoundEventPath = message.HasWonDuel ? "event:/ui/mission/arena_victory" : "event:/ui/campaign/autobattle_defeat",
        });

        OnDuelResult?.Invoke(message.HasWonDuel, message.RatingChange);
    }
}

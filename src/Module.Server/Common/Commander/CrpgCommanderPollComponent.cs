using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Commander;

internal class CrpgCommanderPollComponent : MissionNetwork
{
    public const int MinimumParticipantCountRequired = 3;
    private const int MaximumCommanderVotes = 1;
    public event Action<MissionPeer, MissionPeer, bool>? OnCommanderPollOpened;
    public event Action<MultiplayerPollRejectReason>? OnPollRejected;
    public event Action<int, int, BattleSideEnum>? OnPollUpdated;
    public event Action<CommanderPoll>? OnPollClosed;
    public event Action<CommanderPoll>? OnPollCancelled;
    private readonly List<CommanderPoll> _ongoingPolls = new();
    private readonly Dictionary<NetworkCommunicator, int> _commanderVotesStarted = new();

    private MultiplayerGameNotificationsComponent _notificationsComponent = null!;
    private MissionLobbyComponent _missionLobbyComponent = null!;
    private CrpgCommanderBehaviorServer _commanderBehaviorServer = null!;
    private CrpgCommanderBehaviorClient _commanderBehaviorClient = null!;
    private MultiplayerPollComponent _multiplayerPollComponent = null!;
    private bool _isKickPollOngoing;

    public CommanderPoll? GetCommanderPollBySide(BattleSideEnum? side)
    {
        return _ongoingPolls.FirstOrDefault(c => c.Side == side) ?? null;
    }

    public CommanderPoll? GetCommanderPollByTarget(NetworkCommunicator? target)
    {
        return _ongoingPolls.FirstOrDefault(c => c.Target == target) ?? null;
    }

    public bool IsPollOngoing()
    {
        if (_ongoingPolls.Count > 0)
        {
            return true;
        }

        return false;
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _notificationsComponent = Mission.GetMissionBehavior<MultiplayerGameNotificationsComponent>();
        _missionLobbyComponent = Mission.GetMissionBehavior<MissionLobbyComponent>();
        _multiplayerPollComponent = Mission.GetMissionBehavior<MultiplayerPollComponent>();
        _multiplayerPollComponent.OnKickPollOpened += OnKickPollStarted;
        _multiplayerPollComponent.OnPollCancelled += OnKickPollStopped;
        _multiplayerPollComponent.OnPollClosed += OnKickPollStopped;
        if (GameNetwork.IsServer)
        {
            _commanderBehaviorServer = Mission.Current.GetMissionBehavior<CrpgCommanderBehaviorServer>();
        }
        else if (GameNetwork.IsClient)
        {
            _commanderBehaviorClient = Mission.Current.GetMissionBehavior<CrpgCommanderBehaviorClient>();
        }
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        foreach (CommanderPoll commanderPoll in _ongoingPolls.ToList())
        {
            commanderPoll?.Tick();
        }
    }

    public void Vote(CommanderPoll? poll, bool accepted)
    {
        if (poll == null)
        {
            return;
        }

        if (GameNetwork.IsServer)
        {
            if (GameNetwork.MyPeer != null)
            {
                ApplyVote(GameNetwork.MyPeer, poll, accepted);
            }
        }
        else if (poll.IsOpen)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new CommanderPollResponse { Accepted = accepted });
            GameNetwork.EndModuleEventAsClient();
        }
    }

    public void RequestCommanderPoll(NetworkCommunicator peer, bool isDemoteRequested)
    {
        if (GameNetwork.IsServer)
        {
            if (GameNetwork.MyPeer != null)
            {
                OpenCommanderPollOnServer(GameNetwork.MyPeer, peer, isDemoteRequested);
            }
        }
        else
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new CommanderPollRequested { PlayerPeer = peer, IsDemoteRequested = isDemoteRequested });
            GameNetwork.EndModuleEventAsClient();
        }
    }

    public void RejectPoll(MultiplayerPollRejectReason rejectReason)
    {
        if (!GameNetwork.IsDedicatedServer)
        {
            _notificationsComponent.PollRejected(rejectReason);
        }

        OnPollRejected?.Invoke(rejectReason);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsClient)
        {
            registerer.Register<CommanderPollRequestRejected>(HandleServerEventPollRequestRejected);
            registerer.Register<CommanderPollProgress>(HandleServerEventUpdatePollProgress);
            registerer.Register<CommanderPollCancelled>(HandleServerEventPollCancelled);
            registerer.Register<CommanderPollOpened>(HandleServerEventCommanderPollOpened);
            registerer.Register<CommanderPollClosed>(HandleServerEventCommanderPollClosed);
            return;
        }

        if (GameNetwork.IsServer)
        {
            registerer.Register<CommanderPollResponse>(HandleClientEventPollResponse);
            registerer.Register<CommanderPollRequested>(HandleClientEventCommanderPollRequested);
        }
    }

    private void ApplyVote(NetworkCommunicator peer, CommanderPoll poll, bool accepted)
    {
        if (poll.ApplyVote(peer, accepted))
        {
            List<NetworkCommunicator> pollProgressReceivers = poll.GetPollProgressReceivers();
            int count = pollProgressReceivers.Count;
            for (int i = 0; i < count; i++)
            {
                GameNetwork.BeginModuleEventAsServer(pollProgressReceivers[i]);
                GameNetwork.WriteMessage(new CommanderPollProgress { VotesAccepted = poll.AcceptedCount, VotesRejected = poll.RejectedCount, Side = poll.Side });
                GameNetwork.EndModuleEventAsServer();
            }

            UpdatePollProgress(poll.AcceptedCount, poll.RejectedCount, poll.Side);
        }
    }

    private void RejectPollOnServer(NetworkCommunicator pollCreatorPeer, MultiplayerPollRejectReason rejectReason)
    {
        if (pollCreatorPeer.IsMine)
        {
            RejectPoll(rejectReason);
            return;
        }

        GameNetwork.BeginModuleEventAsServer(pollCreatorPeer);
        GameNetwork.WriteMessage(new CommanderPollRequestRejected { Reason = (int)rejectReason });
        GameNetwork.EndModuleEventAsServer();
    }

    private void UpdatePollProgress(int votesAccepted, int votesRejected, BattleSideEnum side)
    {
        OnPollUpdated?.Invoke(votesAccepted, votesRejected, side);
    }

    private void CancelPoll(CommanderPoll poll)
    {
        poll.Cancel();
        _ongoingPolls.Remove(poll);
        OnPollCancelled?.Invoke(poll);
    }

    private void OnPollCancelledOnServer(CommanderPoll poll)
    {
        List<NetworkCommunicator> pollProgressReceivers = poll.GetPollProgressReceivers();
        int count = pollProgressReceivers.Count;
        for (int i = 0; i < count; i++)
        {
            GameNetwork.BeginModuleEventAsServer(pollProgressReceivers[i]);
            GameNetwork.WriteMessage(new PollCancelled());
            GameNetwork.EndModuleEventAsServer();
        }

        CancelPoll(poll);
    }

    private void OpenCommanderPollOnServer(NetworkCommunicator pollCreatorPeer, NetworkCommunicator targetPeer, bool isDemoteRequested)
    {
        if (_missionLobbyComponent.IsInWarmup)
        {
            RejectPollOnServer(pollCreatorPeer, MultiplayerPollRejectReason.KickPollTargetNotSynced);
            return;
        }

        if (_isKickPollOngoing)
        {
            RejectPollOnServer(pollCreatorPeer, MultiplayerPollRejectReason.HasOngoingPoll);
            return;
        }

        CrpgUser? crpgUser = targetPeer.GetComponent<MissionPeer>().GetComponent<CrpgPeer>().User;
        if (targetPeer.IsMuted || (crpgUser?.Restrictions.Any(r => r.Type == CrpgRestrictionType.Chat) ?? false))
        {
            GameNetwork.BeginModuleEventAsServer(pollCreatorPeer);
            GameNetwork.WriteMessage(new CommanderChatCommand { RejectReason = CommanderChatCommandRejectReason.TargetIsMuted });
            GameNetwork.EndModuleEventAsServer();
            return;
        }

        foreach (CommanderPoll poll in _ongoingPolls)
        {
            if (poll.Side == pollCreatorPeer?.GetComponent<MissionPeer>().Team.Side)
            {
                RejectPollOnServer(pollCreatorPeer, MultiplayerPollRejectReason.HasOngoingPoll);
                return;
            }
        }

        if (pollCreatorPeer != null && pollCreatorPeer.IsConnectionActive && targetPeer.IsConnectionActive)
        {
            if (!targetPeer.IsSynchronized)
            {
                RejectPollOnServer(pollCreatorPeer, MultiplayerPollRejectReason.KickPollTargetNotSynced);
                return;
            }

            MissionPeer component = pollCreatorPeer.GetComponent<MissionPeer>();
            if (component != null)
            {
                if (!_commanderVotesStarted.TryGetValue(pollCreatorPeer, out int _))
                {
                    _commanderVotesStarted.Add(pollCreatorPeer, 0);
                }

                if (_commanderVotesStarted[pollCreatorPeer] >= MaximumCommanderVotes)
                {
                    RejectPollOnServer(pollCreatorPeer, MultiplayerPollRejectReason.TooManyPollRequests);
                    return;
                }

                List<NetworkCommunicator> list = new();
                foreach (NetworkCommunicator networkCommunicator in GameNetwork.NetworkPeers)
                {
                    if (networkCommunicator != null && networkCommunicator != targetPeer && networkCommunicator.IsSynchronized)
                    {
                        MissionPeer? component2 = networkCommunicator.GetComponent<MissionPeer>();
                        if (component2 != null && component2.Team == component.Team)
                        {
                            list.Add(networkCommunicator);
                        }
                    }
                }

                int count = list.Count;
                if (count + 1 >= 2)
                {
                    CommanderPoll poll = OpenCommanderPoll(targetPeer, pollCreatorPeer, isDemoteRequested, list);
                    for (int i = 0; i < count; i++)
                    {
                        GameNetwork.BeginModuleEventAsServer(poll.ParticipantsToVote[i]);
                        GameNetwork.WriteMessage(new CommanderPollOpened { InitiatorPeer = pollCreatorPeer, PlayerPeer = targetPeer, IsDemoteRequested = isDemoteRequested });
                        GameNetwork.EndModuleEventAsServer();
                    }

                    GameNetwork.BeginModuleEventAsServer(targetPeer);
                    GameNetwork.WriteMessage(new CommanderPollOpened { InitiatorPeer = pollCreatorPeer, PlayerPeer = targetPeer, IsDemoteRequested = isDemoteRequested });
                    GameNetwork.EndModuleEventAsServer();
                    _commanderVotesStarted[pollCreatorPeer] += 1;
                    return;
                }

                RejectPollOnServer(pollCreatorPeer, MultiplayerPollRejectReason.NotEnoughPlayersToOpenPoll);
            }
        }
    }

    private CommanderPoll OpenCommanderPoll(NetworkCommunicator targetPeer, NetworkCommunicator pollCreatorPeer, bool isDemoteRequested, List<NetworkCommunicator>? participantsToVote)
    {
        MissionPeer component = pollCreatorPeer.GetComponent<MissionPeer>();
        MissionPeer component2 = targetPeer.GetComponent<MissionPeer>();
        CommanderPoll poll = new(pollCreatorPeer, targetPeer, isDemoteRequested, participantsToVote);
        _ongoingPolls.Add(poll);
        if (GameNetwork.IsServer)
        {
            poll.OnClosedOnServer += OnCommanderPollClosedOnServer;
            poll.OnCancelledOnServer += OnPollCancelledOnServer;
        }

        OnCommanderPollOpened?.Invoke(component, component2, isDemoteRequested);

        if (GameNetwork.MyPeer == pollCreatorPeer)
        {
            Vote(poll, true);
        }

        return poll;
    }

    private void OnCommanderPollClosedOnServer(CommanderPoll poll)
    {
        bool gotEnoughVotes = poll.GotEnoughAcceptVotesToEnd();
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new CommanderPollClosed { PlayerPeer = poll.Target, Accepted = gotEnoughVotes });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        CloseCommanderPoll(gotEnoughVotes, poll);
        if (gotEnoughVotes)
        {
            if (poll.IsDemoteRequested)
            {
                _commanderBehaviorServer.RemoveCommand(poll.Target);
            }
            else
            {
                _commanderBehaviorServer.CreateCommand(poll.Target);
            }
        }
    }

    private void CloseCommanderPoll(bool accepted, CommanderPoll poll)
    {
        poll.Close();
        _ongoingPolls.Remove(poll);

        OnPollClosed?.Invoke(poll);

        if (!GameNetwork.IsDedicatedServer && accepted && poll.Target.IsMine)
        {
            if (poll.IsDemoteRequested)
            {
                InformationManager.DisplayMessage(new InformationMessage
                {
                        Information = new TextObject("{=czWaVzc1}You have been demoted from your Command position!").ToString(),
                        Color = new Color(0.90f, 0.25f, 0.25f),
                });
            }
            else
            {
                InformationManager.DisplayMessage(new InformationMessage
                {
                Information = new TextObject("{=dryVJbMN}You have been chosen to lead as Commander! Use '!o message' to order your troops!").ToString(),
                Color = new Color(0.48f, 0f, 1f),
                });
            }
        }
    }

    private bool HandleClientEventCommanderPollRequested(NetworkCommunicator peer, GameNetworkMessage baseMessage)
    {
        CommanderPollRequested commanderPollRequested = (CommanderPollRequested)baseMessage;
        OpenCommanderPollOnServer(peer, commanderPollRequested.PlayerPeer, commanderPollRequested.IsDemoteRequested);
        return true;
    }

    private bool HandleClientEventPollResponse(NetworkCommunicator peer, GameNetworkMessage baseMessage)
    {
        CommanderPollResponse pollResponse = (CommanderPollResponse)baseMessage;
        CommanderPoll? poll = GetCommanderPollBySide(peer.GetComponent<MissionPeer>().Team.Side);
        if (poll != null)
        {
            ApplyVote(peer, poll, pollResponse.Accepted);
        }

        return true;
    }

    private void HandleServerEventCommanderPollOpened(GameNetworkMessage baseMessage)
    {
        CommanderPollOpened commanderPollOpened = (CommanderPollOpened)baseMessage;
        OpenCommanderPoll(commanderPollOpened.PlayerPeer, commanderPollOpened.InitiatorPeer, commanderPollOpened.IsDemoteRequested, null);
    }

    private void HandleServerEventUpdatePollProgress(GameNetworkMessage baseMessage)
    {
        CommanderPollProgress pollProgress = (CommanderPollProgress)baseMessage;
        UpdatePollProgress(pollProgress.VotesAccepted, pollProgress.VotesRejected, pollProgress.Side);
    }

    private void HandleServerEventPollCancelled(GameNetworkMessage baseMessage)
    {
        CommanderPollCancelled commanderPollCancelled = (CommanderPollCancelled)baseMessage;
        Team team = Mission.MissionNetworkHelper.GetTeamFromTeamIndex(commanderPollCancelled.TeamIndex);
        CommanderPoll? poll = GetCommanderPollBySide(team.Side);
        if (poll != null)
        {
            CancelPoll(poll);
        }
    }

    private void HandleServerEventCommanderPollClosed(GameNetworkMessage baseMessage)
    {
        CommanderPollClosed commanderPollClosed = (CommanderPollClosed)baseMessage;
        CommanderPoll? poll = GetCommanderPollByTarget(commanderPollClosed.PlayerPeer);
        if (poll != null)
        {
            CloseCommanderPoll(commanderPollClosed.Accepted, poll);
        }
    }

    private void HandleServerEventPollRequestRejected(GameNetworkMessage baseMessage)
    {
        CommanderPollRequestRejected pollRequestRejected = (CommanderPollRequestRejected)baseMessage;
        RejectPoll((MultiplayerPollRejectReason)pollRequestRejected.Reason);
    }

    private void OnKickPollStarted(MissionPeer peer1, MissionPeer peer2, bool isBan)
    {
        _isKickPollOngoing = true;
    }

    private void OnKickPollStopped()
    {
        _isKickPollOngoing = false;
    }

    public class CommanderPoll
    {
        public NetworkCommunicator Requester { get; set; }
        public NetworkCommunicator Target { get; set; }
        public event Action<CommanderPoll>? OnClosedOnServer;
        public event Action<CommanderPoll>? OnCancelledOnServer;
        public bool IsDemoteRequested { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }

        public BattleSideEnum Side { get; private set; }
        public List<NetworkCommunicator> ParticipantsToVote { get; } = new List<NetworkCommunicator>();
        public bool IsOpen { get; private set; }
        private int OpenTime { get; }
        private int CloseTime { get; set; }

        public CommanderPoll(NetworkCommunicator requester, NetworkCommunicator target, bool isDemoteRequested, List<NetworkCommunicator>? participantsToVote)
        {
            if (participantsToVote != null)
            {
                ParticipantsToVote = participantsToVote;
            }

            Requester = requester;
            Target = target;
            Side = target.GetComponent<MissionPeer>().Team.Side;
            IsDemoteRequested = isDemoteRequested;
            OpenTime = Environment.TickCount;
            CloseTime = 0;
            AcceptedCount = 0;
            RejectedCount = 0;
            IsOpen = true;
        }

        public virtual bool IsCancelled()
        {
            return false;
        }

        public virtual List<NetworkCommunicator> GetPollProgressReceivers()
        {
            return GameNetwork.NetworkPeers.ToList();
        }

        public void Tick()
        {
            if (GameNetwork.IsServer)
            {
                for (int i = ParticipantsToVote.Count - 1; i >= 0; i--)
                {
                    if (!ParticipantsToVote[i].IsConnectionActive)
                    {
                        ParticipantsToVote.RemoveAt(i);
                    }
                }

                if (IsCancelled())
                {
                    OnCancelledOnServer?.Invoke(this);
                }
                else if (OpenTime < Environment.TickCount - 30000 || ResultsFinalized())
                {
                    OnClosedOnServer?.Invoke(this);
                }
            }
        }

        public void Close()
        {
            CloseTime = Environment.TickCount;
            IsOpen = false;
        }

        public void Cancel()
        {
            Close();
        }

        public bool ApplyVote(NetworkCommunicator peer, bool accepted)
        {
            bool result = false;
            if (ParticipantsToVote.Contains(peer))
            {
                if (accepted)
                {
                    AcceptedCount++;
                }
                else
                {
                    RejectedCount++;
                }

                ParticipantsToVote.Remove(peer);
                result = true;
            }

            return result;
        }

        public bool GotEnoughAcceptVotesToEnd()
        {
            return AcceptedByMajority();
        }

        private bool AcceptedByMajority()
        {
            return (float)AcceptedCount / GetPollParticipantCount() > 0.50001f;
        }

        private int GetPollParticipantCount()
        {
            return AcceptedCount + RejectedCount;
        }

        private bool ResultsFinalized()
        {
            return ParticipantsToVote.Count == 0;
        }
    }
}

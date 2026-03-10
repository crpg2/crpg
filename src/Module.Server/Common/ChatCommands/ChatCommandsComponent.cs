using Crpg.Module.Api;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network;
using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using PlayerMessageAll = NetworkMessages.FromClient.PlayerMessageAll;
using PlayerMessageTeam = NetworkMessages.FromClient.PlayerMessageTeam;
#if CRPG_SERVER
using Crpg.Module.Common.ChatCommands.Admin;
using Crpg.Module.Common.ChatCommands.Commander;
using Crpg.Module.Common.ChatCommands.User;
#endif

namespace Crpg.Module.Common.ChatCommands;

internal class ChatCommandsComponent : GameHandler
{
    public const char CommandPrefix = '!';
    private readonly List<QueuedMessageInfo> _queuedServerMessages;
    private ChatCommand[] _commands = null!;

    public ChatCommandsComponent()
    {
        _queuedServerMessages = new List<QueuedMessageInfo>();
    }

    public void InitChatCommands(ICrpgClient crpgClient)
    {
#if CRPG_SERVER
        _commands = new ChatCommand[]
        {
                new PingCommand(this),
                new SuicideCommand(this),
                new PlayerListCommand(this),
                new KickCommand(this),
                new KillCommand(this),
                new KillAllCommand(this),
                new TeleportCommand(this),
                new AnnouncementCommand(this),
                new MuteCommand(this, crpgClient),
                new BanCommand(this, crpgClient),
                new HotConstantUpdateCommand(this),
                new HotConstantListCommand(this),
                new OrderCommand(this),
                new HelpCommand(this),
                new FriendlyFireInfoCommand(this),
        };
#else
        _commands = Array.Empty<ChatCommand>();
#endif
    }

    public override void OnAfterSave()
    {
    }

    public override void OnBeforeSave()
    {
    }

    public void ServerSendMessageToAdmins(Color color, string message)
    {
        foreach (var peer in GameNetwork.NetworkPeers)
        {
            var user = peer.GetComponent<CrpgPeer>()?.User;
            if (user != null && (user.Role == CrpgUserRole.Admin || user.Role == CrpgUserRole.Moderator))
            {
                ServerSendMessageToPlayer(peer, color, message);
            }
        }
    }

    public void ServerSendMessageToPlayer(NetworkCommunicator targetPlayer, Color color, string message)
    {
        if (!targetPlayer.IsSynchronized)
        {
            _queuedServerMessages.Add(new QueuedMessageInfo(targetPlayer, message));
            return;
        }

        if (!targetPlayer.IsServerPeer && targetPlayer.IsSynchronized)
        {
            GameNetwork.BeginModuleEventAsServer(targetPlayer);
            GameNetwork.WriteMessage(new CrpgServerMessage
            {
                Message = message,
                Red = color.Red,
                Green = color.Green,
                Blue = color.Blue,
                Alpha = color.Alpha,
                IsMessageTextId = false,
            });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    public void ServerSendMessageToPlayer(NetworkCommunicator targetPlayer, string message)
    {
        ServerSendMessageToPlayer(targetPlayer, new Color(1, 1, 1), message);
    }

    public void ServerSendServerMessageToEveryone(Color color, string message)
    {
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new CrpgServerMessage
        {
            Message = message,
            Red = color.Red,
            Green = color.Green,
            Blue = color.Blue,
            Alpha = color.Alpha,
            IsMessageTextId = false,
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients);
    }

#if CRPG_SERVER
    protected override void OnTick(float dt)
    {
        for (int i = 0; i < _queuedServerMessages.Count; i++)
        {
            QueuedMessageInfo queuedMessageInfo = _queuedServerMessages[i];
            if (queuedMessageInfo.SourcePeer.IsSynchronized)
            {
                ServerSendMessageToPlayer(queuedMessageInfo.SourcePeer, queuedMessageInfo.Message);
                _queuedServerMessages.RemoveAt(i);
            }
            else if (queuedMessageInfo.IsExpired)
            {
                _queuedServerMessages.RemoveAt(i);
            }
        }
    }
#endif

    protected override void OnGameNetworkBegin()
    {
        AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
    }

    protected override void OnGameNetworkEnd()
    {
        AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
    }

    private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
    {
        if (!GameNetwork.IsServer)
        {
            return;
        }

        GameNetwork.NetworkMessageHandlerRegisterer handlerRegisterer = new(mode);
        handlerRegisterer.Register<PlayerMessageAll>(HandleClientEventPlayerMessage);
        handlerRegisterer.Register<PlayerMessageTeam>(HandleClientEventPlayerMessage);
    }

    private bool HandleClientEventPlayerMessage(NetworkCommunicator peer, GameNetworkMessage message)
    {
        string chatMessage = string.Empty;
        if (message is PlayerMessageTeam teamMessage)
        {
            chatMessage = teamMessage.Message;
        }
        else if (message is PlayerMessageAll allMessage)
        {
            chatMessage = allMessage.Message;
        }

        if (chatMessage.Length == 0 || chatMessage[0] != CommandPrefix)
        {
            return true;
        }

        string[] tokens = chatMessage.Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            return true;
        }

        string name = tokens[0].ToLowerInvariant();
        var command = _commands.FirstOrDefault(c => c.Name == name);
        if (command == null)
        {
            return true;
        }

        // Set an empty receiver list so no one actually reads this message. (It's nicer than the "You are muted" message.)
        ReflectionHelper.SetProperty(message, "ReceiverList", new List<VirtualPlayer>());
        command.Execute(peer, tokens.Skip(1).ToArray());

        return true;
    }

    private class QueuedMessageInfo
    {
        private const float TimeOutDuration = 3f;

        private readonly DateTime _creationTime;

        public QueuedMessageInfo(NetworkCommunicator sourcePeer, Color color, string message)
        {
            SourcePeer = sourcePeer;
            Message = message;
            Color = color;
            _creationTime = DateTime.Now;
        }

        public QueuedMessageInfo(NetworkCommunicator sourcePeer, string message)
            : this(sourcePeer, new Color(1, 1, 1), message)
        {
        }

        public NetworkCommunicator SourcePeer { get; }
        public string Message { get; }
        public Color Color { get; }
        public bool IsExpired => (DateTime.Now - _creationTime).TotalSeconds >= TimeOutDuration;
    }
}

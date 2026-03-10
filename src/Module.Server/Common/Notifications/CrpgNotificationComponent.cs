using Crpg.Module.Common.Commander;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Notifications;

/// <summary>
/// Used for notifications which a shared between gamemodes (!a command etc.).
/// Also includes the Native MultiplayerGameNotificationsComponent class.
/// </summary>
internal class CrpgNotificationComponent : MultiplayerGameNotificationsComponent
{
    private CrpgCommanderBehaviorClient? _commanderClient;
    private ChatBox? _chatBox;
    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        if (GameNetwork.IsClient)
        {
            _commanderClient = Mission.Current.GetMissionBehavior<CrpgCommanderBehaviorClient>();
            _chatBox = Game.Current.GetGameHandler<ChatBox>();
        }
    }

    protected override void AddRemoveMessageHandlers(
        GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
            registerer.Register<CrpgNotification>(HandleNotification);
            registerer.Register<CrpgNotificationId>(HandleNotificationId);
            registerer.Register<CrpgServerMessage>(HandleServerMessage);
        }
    }

    private void HandleNotification(CrpgNotification notification)
    {
        PrintNotification(notification.Message, notification.Type, notification.SoundEvent);
    }

    private void HandleNotificationId(CrpgNotificationId notification)
    {
        foreach (var v in notification.Variables)
        {
            // It's so fucking weird but the variable are set on a static context. I guess it works because everything
            // text related is done only on the game loop thread.
            GameTexts.SetVariable(v.Key, v.Value);
        }

        string message = GameTexts.FindText(notification.TextId, notification.TextVariation).ToString();
        PrintNotification(message, notification.Type, notification.SoundEvent);
    }

    private void HandleServerMessage(CrpgServerMessage message)
    {
        string msg = message.IsMessageTextId ? GameTexts.FindText(message.Message).ToString() : message.Message;
        InformationManager.DisplayMessage(new InformationMessage(msg, new Color(message.Red, message.Green, message.Blue, message.Alpha)));
    }

    private void PrintNotification(string message, CrpgNotificationType type, string? soundEvent)
    {
        if (type == CrpgNotificationType.Notification) // Small text at the top of the screen.
        {
            MBInformationManager.AddQuickInformation(new TextObject(message), 0, null, null, soundEvent);
        }
        else if (type == CrpgNotificationType.Announcement) // Big red text in the middle of the screen.
        {
            InformationManager.AddSystemNotification(message);
        }
        else if (type == CrpgNotificationType.Sound)
        {
            SoundEvent.CreateEventFromString(soundEvent, Mission.Scene).Play();
        }
        else if (type == CrpgNotificationType.Commander && _commanderClient != null) // Chatbox display message
        {
            BattleSideEnum side = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.Team?.Side ?? BattleSideEnum.None;
            NetworkCommunicator? myCommander = _commanderClient.GetCommanderBySide(side);

            if (myCommander != null && _chatBox != null)
            {
                PlayerId id = myCommander.VirtualPlayer.Id;
                if (_chatBox.IsPlayerMutedFromGame(id) || PermaMuteList.IsPlayerMuted(id) || _chatBox.IsPlayerMutedFromPlatform(id))
                {
                    return;
                }
            }

            InformationManager.DisplayMessage(new InformationMessage
            {
                Information = new TextObject("{=iERprCDU}(Commander) {NAME}: ",
                    new Dictionary<string, object> { ["NAME"] = myCommander?.UserName ?? string.Empty }) + message,
                Color = new Color(0.1f, 1f, 0f),
                SoundEventPath = "event:/ui/mission/horns/attack",
            });

            BasicCharacterObject? commanderCharacterObject = _commanderClient.GetCommanderCharacterObjectBySide(side);
            MBInformationManager.AddQuickInformation(new TextObject(message), 5000, commanderCharacterObject, commanderCharacterObject?.FirstBattleEquipment, soundEvent);
        }
    }
}

using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.FriendlyFireReport;

internal class FriendlyFireReportClientBehavior : MissionNetwork
{
    private int _reportWindowSeconds = 0; // default unlimited, updated via FriendlyFireHitMessage
    private bool _ctrlMWasPressed;
    private DateTime? _lastHitMessageTime;
    private int? _lastAttackerAgentIndex;
    private string _lastAttackerName = "Unknown";
    private bool _expiredMessageShown;

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        bool isCtrlDown = Input.IsKeyDown(InputKey.LeftControl) || Input.IsKeyDown(InputKey.RightControl);
        bool isMPressed = Input.IsKeyPressed(InputKey.M);

        if (isCtrlDown && isMPressed && !_ctrlMWasPressed) // ctrl+m pressed
        {
            _ctrlMWasPressed = true;

            if (_lastHitMessageTime != null)
            {
                if (_reportWindowSeconds > 0)
                {
                    double elapsedSeconds = (DateTime.UtcNow - _lastHitMessageTime.Value).TotalSeconds;
                    if (elapsedSeconds > _reportWindowSeconds)
                    {
                        if (!_expiredMessageShown)
                        {
                            // InformationManager.DisplayMessage(new InformationMessage($"[FF] Time expired to report {_lastAttackerName} for teamhit.", Colors.Yellow));
                            // TextObject windowExpiredText = GameTexts.FindText("str_ff_teamhit_msg_window_expired");
                            TextObject windowExpiredText = new("{=KgZprgXA}Time expired to report {ATTACKER} for teamhit.");
                            windowExpiredText.SetTextVariable("ATTACKER", _lastAttackerName);

                            _expiredMessageShown = true;
                        }

                        // Ensure state is cleared regardless of whether the message was shown this frame
                        _lastHitMessageTime = null;
                        _lastAttackerAgentIndex = null;
                        _lastAttackerName = "Unknown";

                        return; // Don't report if window expired
                    }
                }

                // Report is valid
                HandleCtrlMPressed();
            }

            // Always clear state after attempt (successful or expired)
            _lastHitMessageTime = null;
            _lastAttackerAgentIndex = null;
            _expiredMessageShown = false;
            _lastAttackerName = "Unknown";
        }

        if (!isCtrlDown || !Input.IsKeyDown(InputKey.M))
        {
            _ctrlMWasPressed = false;
        }
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsClient)
        {
            base.AddRemoveMessageHandlers(registerer);
            registerer.Register<FriendlyFireHitMessage>(HandleFriendlyFireHitMessage);
            registerer.Register<FriendlyFireNotificationMessage>(HandleFriendlyFireTextMessage);
        }
    }

    private void HandleCtrlMPressed()
    {
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new FriendlyFireReportClientMessage());
        GameNetwork.EndModuleEventAsClient();
    }

    private void HandleFriendlyFireHitMessage(FriendlyFireHitMessage message)
    {
        // Update _reportWindowSeconds set in Crpg.serverConfiguration
        _reportWindowSeconds = message.ReportWindow;
        _lastAttackerAgentIndex = message.AttackerAgentIndex;
        _expiredMessageShown = false;

        if (_lastAttackerAgentIndex == null || Mission.Current == null)
        {
            return;
        }

        Agent agent = Mission.Current.FindAgentWithIndex((int)_lastAttackerAgentIndex);
        if (agent == null)
        {
            return;
        }

        _lastAttackerName = agent?.Name?.ToString() ?? "Unknown";

        if (_reportWindowSeconds <= 0) // no window
        {
            // InformationManager.DisplayMessage(new InformationMessage($"[FF] Team hit by {_lastAttackerName} (Dmg: {message.Damage}). Press Ctrl+M to mark that you believe this was intentional.", Colors.Yellow));
            // TextObject windowExpiredText = GameTexts.FindText("str_ff_teamhit_msg_report_prompt");
            TextObject reportPrompText = new("{=WH15BANu}Team hit by {ATTACKER} (Dmg: {DAMAGE}). Press Ctrl+M if you believe this was intentional.");
            reportPrompText.SetTextVariable("ATTACKER", _lastAttackerName);
            reportPrompText.SetTextVariable("DAMAGE", message.Damage);
        }
        else if (_reportWindowSeconds > 0) // has window
        {
            // InformationManager.DisplayMessage(new InformationMessage($"[FF] Team hit by {_lastAttackerName} (Dmg: {message.Damage}). Press Ctrl+M to mark that you believe this was intentional {_reportWindowSeconds} seconds remaining."));
            // TextObject windowExpiredText = GameTexts.FindText("str_ff_teamhit_msg_report_prompt_window");
            TextObject reportPrompNoTimeText = new("{=KORWOuGO}Team hit by {ATTACKER} (Dmg: {DAMAGE}). Press Ctrl+M if you believe this was intentional {TIMELEFT} seconds remaining.");
            reportPrompNoTimeText.SetTextVariable("ATTACKER", _lastAttackerName);
            reportPrompNoTimeText.SetTextVariable("DAMAGE", message.Damage);
            reportPrompNoTimeText.SetTextVariable("TIMELEFT", _reportWindowSeconds);
            InformationManager.DisplayMessage(new InformationMessage(reportPrompNoTimeText.ToString(), Colors.Red));
        }

        // New team hit → allow a fresh Ctrl+M
        _ctrlMWasPressed = false;
        // Set the timer for when the report window opens
        _lastHitMessageTime = DateTime.UtcNow;
    }

    private void HandleFriendlyFireTextMessage(FriendlyFireNotificationMessage message)
    {
        FriendlyFireMessageMode mode = message.Mode;
        Color msgColor;
        switch (mode)
        {
            case FriendlyFireMessageMode.Default:
                msgColor = Colors.Yellow;
                break;
            case FriendlyFireMessageMode.TeamDamageReportForVictim:
                msgColor = Colors.Red;
                break;
            case FriendlyFireMessageMode.TeamDamageReportForAdmins:
                msgColor = Colors.Magenta;
                break;
            case FriendlyFireMessageMode.TeamDamageReportForAttacker:
                msgColor = Colors.Yellow;
                break;
            case FriendlyFireMessageMode.TeamDamageReportKick:
                msgColor = Colors.Magenta;
                break;
            case FriendlyFireMessageMode.TeamDamageReportError:
                msgColor = Colors.Yellow;
                break;
            default:
                msgColor = Colors.Yellow;
                break;
        }

        InformationManager.DisplayMessage(new InformationMessage(message.Message, msgColor));
    }
}

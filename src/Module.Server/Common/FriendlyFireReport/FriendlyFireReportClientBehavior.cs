using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.FriendlyFireReport;

internal class FriendlyFireReportClientBehavior : MissionNetwork, IUseKeyBinder
{
    private static readonly string KeyCategoryId = KeyBinder.KeyBinder.Categories.CrpgGeneral.CategoryId;
    private int _reportWindowSeconds; // default unlimited, updated via FriendlyFireHitMessage
    private bool _ctrlMWasPressed;
    private DateTime? _lastHitMessageTime;
    private int? _lastAttackerAgentIndex;
    private string _lastAttackerName = "Unknown";
    private bool _expiredMessageShown;

    BindedKeyCategory IUseKeyBinder.BindedKeys => new()
    {
        CategoryId = KeyCategoryId,
        Category = KeyBinder.KeyBinder.Categories.CrpgGeneral.CategoryName,
        Keys = new List<BindedKey>
        {
            new()
            {
                Id = "key_report_team_hit",
                Name = "Report Team Hit",
                Description = "Report a team hit you received",
                DefaultInputKey = InputKey.M,
            },
        },
    };

    private GameKey? reportTeamHitKey;
    private GameKey? commandModifierKey;

    public override void EarlyStart()
    {
        reportTeamHitKey = HotKeyManager.GetCategory(KeyCategoryId).RegisteredGameKeys.Find(gk => gk != null && gk.StringId == "key_report_team_hit");
        commandModifierKey = HotKeyManager.GetCategory(KeyCategoryId).RegisteredGameKeys.Find(gk => gk != null && gk.StringId == "key_command_modifier");
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        bool isCmdModifierDown = commandModifierKey != null && (Input.IsKeyDown(commandModifierKey.KeyboardKey.InputKey) || Input.IsKeyDown(commandModifierKey.ControllerKey.InputKey));
        bool isMPressed = reportTeamHitKey != null && (Input.IsKeyPressed(reportTeamHitKey.KeyboardKey.InputKey) || Input.IsKeyPressed(reportTeamHitKey.ControllerKey.InputKey));

        if (isCmdModifierDown && isMPressed && !_ctrlMWasPressed) // ctrl+m pressed
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

        if (!isCmdModifierDown || !isMPressed)
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

        _lastAttackerName = agent?.Name ?? "Unknown";
        string cmdModifierKeyStr = commandModifierKey?.KeyboardKey.InputKey.ToString() ?? "cmd modifier";
        string mKeyStr = reportTeamHitKey?.KeyboardKey.InputKey.ToString() ?? "report key";

        if (_reportWindowSeconds <= 0) // no window
        {
            TextObject reportPrompText = new("{=WH15BANu}Team hit by {ATTACKER} (Dmg: {DAMAGE}). Press [{MODIFIERKEY}+{REPORTKEY}] if you believe this was intentional.");
            reportPrompText.SetTextVariable("ATTACKER", _lastAttackerName);
            reportPrompText.SetTextVariable("DAMAGE", message.Damage);
            reportPrompText.SetTextVariable("MODIFIERKEY", cmdModifierKeyStr);
            reportPrompText.SetTextVariable("REPORTKEY", mKeyStr);
            InformationManager.DisplayMessage(new InformationMessage(reportPrompText.ToString(), Colors.Red));
        }
        else if (_reportWindowSeconds > 0) // has window
        {
            TextObject reportPrompNoTimeText = new("{=KORWOuGO}Team hit by {ATTACKER} (Dmg: {DAMAGE}). Press [{MODIFIERKEY}+{REPORTKEY}] if you believe this was intentional {TIMELEFT} seconds remaining.");
            reportPrompNoTimeText.SetTextVariable("ATTACKER", _lastAttackerName);
            reportPrompNoTimeText.SetTextVariable("DAMAGE", message.Damage);
            reportPrompNoTimeText.SetTextVariable("TIMELEFT", _reportWindowSeconds);
            reportPrompNoTimeText.SetTextVariable("MODIFIERKEY", cmdModifierKeyStr);
            reportPrompNoTimeText.SetTextVariable("REPORTKEY", mKeyStr);
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
                msgColor = Colors.Cyan;
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

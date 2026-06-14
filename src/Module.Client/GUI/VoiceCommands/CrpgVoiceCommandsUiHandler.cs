using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace Crpg.Module.GUI.VoiceCommands;

internal class CrpgVoiceCommandsUiHandler : MissionView
{
    private const float MenuTimeoutSeconds = 8f;

    private readonly List<VoiceCommandMenuItem> _menuStack = new();
    private CrpgVoiceCommandsVm _dataSource;
    private GauntletLayer? _gauntletLayer;
    private float _timeSinceMenuInput;
    private bool _menuFocused;
    // Ticks remaining before we actually release the gauntlet layer focus after a shout fires.
    // Shouts detect key release, so this is 1: hold focus through the current tick so the
    // scene layer stays blocked, then release on the next tick when T/Y is no longer "just released".
    private int _releaseFocusAfterTicks;

    public CrpgVoiceCommandsUiHandler()
    {
        _dataSource = new CrpgVoiceCommandsVm();
        ViewOrderPriority = 5;
    }

    public override void OnMissionScreenInitialize()
    {
        _dataSource = new CrpgVoiceCommandsVm();
        // No IsFocusLayer here — setting it at creation auto-steals focus
        // from the scene layer, preventing any key detection before the menu opens.
        _gauntletLayer = new GauntletLayer("VoiceCommands", ViewOrderPriority);
        _gauntletLayer.LoadMovie("VoiceCommands", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
        base.OnMissionScreenInitialize();
    }

    public override void OnMissionScreenFinalize()
    {
        if (_gauntletLayer != null)
        {
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }

        _dataSource.OnFinalize();
        _dataSource = null!;
        base.OnMissionScreenFinalize();
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);

        // After a shout fires we keep the gauntlet layer focused for a couple of ticks so
        // the scene layer remains blocked while the shout key transitions from pressed to
        // released.  Without this, the key-release event is processed by GauntletChatLogView
        // (which reads SceneLayer.Input directly) and opens the chat box.
        if (_releaseFocusAfterTicks > 0)
        {
            _releaseFocusAfterTicks--;
            if (_releaseFocusAfterTicks == 0)
            {
                ReleaseFocus();
            }

            return;
        }

        Agent? mainAgent = Agent.Main;
        if (mainAgent == null || !mainAgent.IsActive())
        {
            if (_menuFocused)
            {
                _menuStack.Clear();
                _dataSource.HideMenu();
                ReleaseFocus();
            }

            return;
        }

        if (_menuStack.Count == 0)
        {
            // Scene layer still has focus here — use base Input.
            // Use key release so that releasing Q here doesn't immediately
            // also navigate to Q=Quick on the same frame the menu opens.
            if (Input.IsKeyReleased(CrpgVoiceCommands.Root.Key))
            {
                _menuStack.Add(CrpgVoiceCommands.Root);
                RefreshMenu();
            }

            return;
        }

        // Menu is open and we called TrySetFocus(_gauntletLayer) in RefreshMenu,
        // so keypresses are now routed to the gauntlet layer's input context.
        IInputContext input = _gauntletLayer!.Input;

        _timeSinceMenuInput += dt;
        if (_timeSinceMenuInput >= MenuTimeoutSeconds || input.IsKeyPressed(InputKey.Escape))
        {
            CloseMenu();
            return;
        }

        if (input.IsKeyPressed(InputKey.BackSpace))
        {
            _menuStack.RemoveAt(_menuStack.Count - 1);
            if (_menuStack.Count == 0)
            {
                CloseMenu();
            }
            else
            {
                RefreshMenu();
            }

            return;
        }

        // Forward WASD movement to the agent even though our layer is blocking other keyboard
        // input. MissionMainAgentController (ViewOrderPriority 0) has already zeroed
        // MovementInputVector this tick because its base.Input is blocked — we override it here.
        {
            float mx = 0f, my = 0f;
            if (input.IsKeyDown(InputKey.W)) my += 1f;
            if (input.IsKeyDown(InputKey.S)) my -= 1f;
            if (input.IsKeyDown(InputKey.D)) mx += 1f;
            if (input.IsKeyDown(InputKey.A)) mx -= 1f;
            mainAgent.MovementInputVector = new Vec2(mx, my);
        }

        // All navigation uses key-release so that pressing a key to reach a category doesn't
        // immediately trigger a shout on that same key's release in the new submenu.
        foreach (var item in _menuStack[_menuStack.Count - 1].Children)
        {
            if (!input.IsKeyReleased(item.Key))
            {
                continue;
            }

            if (item.IsCategory)
            {
                _menuStack.Add(item);
                RefreshMenu();
            }
            else
            {
                // GauntletChatLogView also triggers on T/Y release via SceneLayer.Input.
                // Our gauntlet layer still holds focus on this tick so the scene layer is
                // blocked. Delay releasing focus by one tick to keep it blocked through here.
                mainAgent.MakeVoice(
                    new SkinVoiceManager.SkinVoiceType(item.VoiceName!),
                    SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
                _menuStack.Clear();
                _dataSource.HideMenu();
                _timeSinceMenuInput = 0f;
                _releaseFocusAfterTicks = 1;
            }

            return;
        }
    }

    private void RefreshMenu()
    {
        _timeSinceMenuInput = 0f;
        string title = string.Join(" > ", _menuStack.Select(n => n.Label));
        _dataSource.ShowMenu(title, _menuStack[_menuStack.Count - 1].Children);

        if (!_menuFocused && _gauntletLayer != null)
        {
            _menuFocused = true;
            _gauntletLayer.IsFocusLayer = true;
            _gauntletLayer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.Keyboardkeys);
            ScreenManager.TrySetFocus(_gauntletLayer);
        }
    }

    private void CloseMenu()
    {
        _menuStack.Clear();
        _dataSource.HideMenu();
        ReleaseFocus();
    }

    private void ReleaseFocus()
    {
        if (_menuFocused && _gauntletLayer != null)
        {
            _menuFocused = false;
            _gauntletLayer.IsFocusLayer = false;
            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            ScreenManager.TryLoseFocus(_gauntletLayer);
        }
    }
}

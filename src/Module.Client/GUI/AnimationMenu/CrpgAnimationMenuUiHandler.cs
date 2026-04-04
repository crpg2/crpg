using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.AnimationMenu;

/// <summary>
/// MissionView that owns the emote menu Gauntlet layer.
///
/// Responsibilities:
///   • Registers the "Open Emote Menu" keybind via cRPG's KeyBinder system
///     (default: B — players can rebind in Options → Key Bindings → cRPG General).
///   • Loads CrpgAnimationMenu.xml into a GauntletLayer on mission start.
///   • Toggles the menu open/closed on keypress, handing cursor focus to the layer.
///   • Closes the menu automatically when the player's agent dies or becomes inactive.
///   • Tears down cleanly on mission end.
///
/// INTEGRATION: Add  new CrpgAnimationMenuUiHandler()  to the MissionView[] array
/// in every game-mode file that should support emotes, e.g.:
///
///     // CrpgBattleGameMode.cs, CrpgSiegeGameMode.cs, CrpgDuelGameMode.cs, …
///     new CrpgAnimationMenuUiHandler(),
///
/// Place it after CrpgAgentHud so the emote panel draws on top.
/// </summary>
internal class CrpgAnimationMenuUiHandler : MissionView, IUseKeyBinder
{
    private const string OpenMenuKeyId = "key_open_emote_menu";
    private static readonly string KeyCategoryId = KeyBinder.Categories.CrpgGeneral.CategoryId;

    private CrpgAnimationMenuVm? _dataSource;
    private GauntletLayer? _gauntletLayer;
    private GameKey? _openMenuKey;

    // ──────────────────────────────────────────────────────────────────────────
    // IUseKeyBinder — KeyBinder.AutoRegister() picks this up automatically.
    // ──────────────────────────────────────────────────────────────────────────

    BindedKeyCategory IUseKeyBinder.BindedKeys => new()
    {
        CategoryId = KeyCategoryId,
        Category = KeyBinder.Categories.CrpgGeneral.CategoryName,
        Keys = new List<BindedKey>
        {
            new()
            {
                Id = OpenMenuKeyId,
                Name = "{=CrpgEmoteMenuKey}Emote Menu",
                Description = "{=CrpgEmoteMenuKeyDesc}Open/close the emote animation menu",
                DefaultInputKey = InputKey.B,
            },
        },
    };

    public CrpgAnimationMenuUiHandler()
    {
        ViewOrderPriority = 3; // draw above CrpgAgentHud (priority 2)
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MissionView lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();

        _dataSource = new CrpgAnimationMenuVm();
        _gauntletLayer = new GauntletLayer("CrpgAnimationMenu", ViewOrderPriority);
        _gauntletLayer.LoadMovie("CrpgAnimationMenu", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
    }

    public override void EarlyStart()
    {
        base.EarlyStart();

        // Resolve the GameKey after HotKeyManager has been fully initialised.
        _openMenuKey = HotKeyManager
            .GetCategory(KeyCategoryId)?
            .RegisteredGameKeys?
            .Find(gk => gk != null && gk.StringId == OpenMenuKeyId);

        // Push a readable key-name hint into the VM so the XML prefab can display it.
        if (_openMenuKey != null && _dataSource != null)
        {
            string keyLabel = _openMenuKey.KeyboardKey.InputKey.ToString();
            _dataSource.OpenMenuKeyName = $"[{keyLabel}] Emote Menu";
        }
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);

        if (_dataSource == null || _openMenuKey == null)
        {
            return;
        }

        // Auto-close if the agent dies or leaves.
        bool agentAlive = Mission.Current?.MainAgent?.IsActive() == true;
        if (!agentAlive && _dataSource.IsEnabled)
        {
            CloseMenu();
            return;
        }

        // Toggle on keypress (keyboard or controller).
        bool keyPressed = Input.IsKeyPressed(_openMenuKey.KeyboardKey.InputKey)
                       || Input.IsKeyPressed(_openMenuKey.ControllerKey.InputKey);
        if (keyPressed)
        {
            if (_dataSource.IsEnabled)
            {
                CloseMenu();
            }
            else if (agentAlive)
            {
                OpenMenu();
            }
        }

        // Allow Escape to close the menu while it is open.
        if (_dataSource.IsEnabled && _gauntletLayer!.Input.IsHotKeyReleased("ToggleEscapeMenu"))
        {
            CloseMenu();
        }
    }

    public override void OnMissionScreenFinalize()
    {
        if (_gauntletLayer != null)
        {
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }

        _dataSource?.OnFinalize();
        _dataSource = null;
        base.OnMissionScreenFinalize();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private void OpenMenu()
    {
        _dataSource!.ToggleMenu(); // sets IsEnabled = true, resets to category page
        _gauntletLayer!.IsFocusLayer = true;
        ScreenManager.TrySetFocus(_gauntletLayer);
        MissionScreen.SetDisplayDialog(true);
    }

    private void CloseMenu()
    {
        _dataSource!.CloseMenu();
        _gauntletLayer!.IsFocusLayer = false;
        MissionScreen.SetDisplayDialog(false);
    }
}

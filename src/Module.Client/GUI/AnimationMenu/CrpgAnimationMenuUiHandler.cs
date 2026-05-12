using Crpg.Module.Common;
using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.AnimationMenu;

/// <summary>
/// MissionView that owns the animation menu Gauntlet layer.
///
/// Responsibilities:
///   • Registers the "Open Animation Menu" keybind via cRPG's KeyBinder system
///     (default: B — players can rebind in Options → Key Bindings → cRPG General).
///   • Loads CrpgAnimationMenu.xml into a GauntletLayer on mission start.
///   • Toggles the menu open/closed on keypress, handing cursor focus to the layer.
///   • Closes the menu automatically when the player's agent dies or becomes inactive.
///   • Tears down cleanly on mission end.
///
/// INTEGRATION: Add  new CrpgAnimationMenuUiHandler()  to the MissionView[] array
/// in every game-mode file that should support animations, e.g.:
///
///     // CrpgBattleGameMode.cs, CrpgSiegeGameMode.cs, CrpgDuelGameMode.cs, …
///     new CrpgAnimationMenuUiHandler(),
///
/// Place it after CrpgAgentHud so the animation panel draws on top.
/// </summary>
internal class CrpgAnimationMenuUiHandler : MissionView, IUseKeyBinder
{
    private const string OpenMenuKeyId = "key_open_animation_menu";
    private static readonly string KeyCategoryId = KeyBinder.Categories.CrpgGeneral.CategoryId;

    private CrpgAnimationMenuVm? _dataSource;
    private GauntletLayer? _gauntletLayer;
    private GameKey? _openMenuKey;
    private CrpgAnimationBehavior? _animationBehavior;
    private MissionLobbyComponent? _lobbyComponent;

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
                Name = "{=CrpgAnimationMenuKey}Animation Menu",
                Description = "{=CrpgAnimationMenuKeyDesc}Open/close the animation menu",
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

        _animationBehavior = Mission.GetMissionBehavior<CrpgAnimationBehavior>();
        _lobbyComponent = Mission.GetMissionBehavior<MissionLobbyComponent>();
        _dataSource = new CrpgAnimationMenuVm();
        _dataSource.CloseRequested += CloseMenu;
        _dataSource.AnimRequested += OnAnimRequested;
        _gauntletLayer = new GauntletLayer("CrpgAnimationMenu", ViewOrderPriority);
        _gauntletLayer.LoadMovie("CrpgAnimationMenu", _dataSource);
        MissionScreen.AddLayer(_gauntletLayer);
        _lobbyComponent?.OnPostMatchEnded += HandlePostMatchEnded;
    }

    public override void EarlyStart()
    {
        base.EarlyStart();

        _openMenuKey = HotKeyManager
            .GetCategory(KeyCategoryId)?
            .RegisteredGameKeys?
            .Find(gk => gk != null && gk.StringId == OpenMenuKeyId);
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);

        if (_dataSource == null || _openMenuKey == null)
        {
            return;
        }

        bool agentAlive = Mission.Current?.MainAgent?.IsActive() == true;
        if (!agentAlive && _dataSource.IsEnabled)
        {
            CloseMenu();
            return;
        }

        if (!_dataSource.IsEnabled)
        {
            bool keyPressed = Input.IsKeyPressed(_openMenuKey.KeyboardKey.InputKey)
                           || Input.IsKeyPressed(_openMenuKey.ControllerKey.InputKey);
            if (keyPressed && agentAlive)
            {
                OpenMenu();
            }

            return;
        }

        if (Input.IsKeyPressed(InputKey.Escape))
        {
            CloseMenu();
            return;
        }

        if (_gauntletLayer?.Input.IsKeyReleased(InputKey.RightMouseButton) == true
            || Input.IsKeyReleased(InputKey.RightMouseButton))
        {
            _dataSource.ExecuteBack();
            return;
        }

        if (Input.IsKeyPressed(_openMenuKey.KeyboardKey.InputKey)
            || Input.IsKeyPressed(_openMenuKey.ControllerKey.InputKey))
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

        _lobbyComponent?.OnPostMatchEnded -= HandlePostMatchEnded;

        if (_dataSource != null)
        {
            _dataSource.CloseRequested -= CloseMenu;
            _dataSource.AnimRequested -= OnAnimRequested;
            _dataSource.OnFinalize();
            _dataSource = null;
        }

        base.OnMissionScreenFinalize();
    }

    public override void OnAgentMount(Agent agent)
    {
        base.OnAgentMount(agent);
        if (agent == Mission.Current?.MainAgent)
        {
            _dataSource?.RefreshCurrentPage();
        }
    }

    public override void OnAgentDismount(Agent agent)
    {
        base.OnAgentDismount(agent);
        if (agent == Mission.Current?.MainAgent)
        {
            _dataSource?.RefreshCurrentPage();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private void OnAnimRequested(CrpgAnimEntry anim)
    {
        _animationBehavior?.RequestPlayAnimation(anim.ActionId);
    }

    private void OpenMenu()
    {
        if (_animationBehavior?.IsEnabled == false)
        {
            InformationManager.DisplayMessage(new InformationMessage(new TextObject(
                "{=usDfC299}Animations are disabled on this server.").ToString(), Colors.Red));
            return;
        }

        _dataSource?.ToggleMenu();
        _gauntletLayer?.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
    }

    private void CloseMenu()
    {
        _dataSource?.CloseMenu();
        _gauntletLayer?.InputRestrictions.ResetInputRestrictions();
    }

    private void HandlePostMatchEnded()
    {
        CloseMenu();
    }
}

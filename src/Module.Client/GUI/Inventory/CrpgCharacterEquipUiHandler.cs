using Crpg.Module.Common;
using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using Crpg.Module.Common.Network.Armory;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace Crpg.Module.GUI.Inventory;

public class CrpgCharacterEquipUiHandler : MissionView, IUseKeyBinder
{
    private static readonly TimeSpan ApiRefreshCooldown = TimeSpan.FromSeconds(10);
    internal static CrpgConstants Constants { get; private set; } = null!;
    internal bool IsCharacterEquipVisible => _characterEquipVm?.IsVisible ?? false;
    private bool _firstOpen = true;
    private DateTime _lastApiRefreshTime = DateTime.MinValue;
    private Action<object?>? _onVmCloseHandler;
    private Action<object?>? _onVmSelectedReadyHandler;
    private Action<object?>? _onVmCloseDestoryHandler;

    private static readonly string KeyCategoryId = KeyBinder.Categories.CrpgGeneral.CategoryId;

    private CrpgCharacterLoadoutBehaviorClient? _userLoadout;
    private CrpgClanArmoryClient? _clanArmory;
    private MissionLobbyComponent? _lobbyComponent;
    private CrpgTeamInventoryClient? _teamInventory;
    private GauntletLayer? _characterEquipLayer;
    private CrpgCharacterEquipVM? _characterEquipVm;
    private GauntletMovieIdentifier? _characterEquipMovieId;

    private GameKey? _toggleEquipmentSelectKey;

    BindedKeyCategory IUseKeyBinder.BindedKeys => new()
    {
        CategoryId = KeyCategoryId,
        Category = KeyBinder.Categories.CrpgGeneral.CategoryName,
        Keys = new List<BindedKey>
        {
            new()
            {
                Id = "key_toggle_equip_select",
                Name = new TextObject("{=KC9dx127}Open Character/Equipment Manager").ToString(),
                Description = new TextObject("{=KC9dx128}Open character/equipment manager gui").ToString(),
                DefaultInputKey = InputKey.I,
            },
        },
    };

    public override void OnMissionScreenInitialize()
    {
        base.OnMissionScreenInitialize();
        _onVmCloseHandler = OnVmCloseRequested;
        _onVmCloseDestoryHandler = OnVmCloseDestroyRequested;
        _onVmSelectedReadyHandler = OnVmSelectedReadyRequested;
        _teamInventory = Mission.Current.GetMissionBehavior<CrpgTeamInventoryClient>();
        _userLoadout = Mission.Current.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        _clanArmory = Mission.Current.GetMissionBehavior<CrpgClanArmoryClient>();
        _lobbyComponent = Mission.Current.GetMissionBehavior<MissionLobbyComponent>();

        if (_userLoadout != null)
        {
            _userLoadout.OnUserCharacterLoadoutEnabledChanged += HandleUserLoadoutEnabledChanged;
            _userLoadout.OnForceOpenEquipMenu += HandleForceEquipMenu;
        }

        if (_teamInventory != null)
        {
            _teamInventory.OnTeamInventoryEnabledChanged += HandleTeamInventoryEnabledChanged;
            _teamInventory.OnForceOpenEquipMenu += HandleForceEquipMenu;
        }

        if (_lobbyComponent != null)
        {
            _lobbyComponent.OnPostMatchEnded += HandlePostMatchEnded;
        }
    }

    public override void OnMissionScreenFinalize()
    {
        base.OnMissionScreenFinalize();
        if (_userLoadout != null)
        {
            _userLoadout.OnUserCharacterLoadoutEnabledChanged -= HandleUserLoadoutEnabledChanged;
            _userLoadout.OnForceOpenEquipMenu -= HandleForceEquipMenu;
        }

        if (_teamInventory != null)
        {
            _teamInventory.OnTeamInventoryEnabledChanged -= HandleTeamInventoryEnabledChanged;
            _teamInventory.OnForceOpenEquipMenu -= HandleForceEquipMenu;
        }

        if (_lobbyComponent != null)
        {
            _lobbyComponent.OnPostMatchEnded -= HandlePostMatchEnded;
        }

        HideCharacterEquip(destroy: true);
    }

    public override void EarlyStart()
    {
        base.EarlyStart();
        _toggleEquipmentSelectKey = HotKeyManager.GetCategory(KeyCategoryId).RegisteredGameKeys.Find(gk => gk != null && gk.StringId == "key_toggle_equip_select");
    }

    public override void OnMissionScreenTick(float dt)
    {
        base.OnMissionScreenTick(dt);

        _characterEquipVm?.TickStatusMessages(dt); // update status messages in gui

        bool characterEquipOpen = _characterEquipVm?.IsVisible ?? false;

        bool escapePressed = false;
        if (_characterEquipLayer != null && _characterEquipLayer.IsFocusLayer)
        {
            escapePressed = _characterEquipLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") ||
                            _characterEquipLayer.Input.IsHotKeyReleased("Exit");
        }

        if (characterEquipOpen && escapePressed)
        {
            HideCharacterEquip();
            return;
        }

        if (_toggleEquipmentSelectKey != null && Input.IsKeyPressed(_toggleEquipmentSelectKey.KeyboardKey.InputKey))
        {
            ToggleCharacterEquip();
            return;
        }
    }

    // called from game mode
    internal static void Configure(CrpgConstants constants)
    {
        Constants = constants;
    }

    private void OnVmCloseRequested(object? sender) => HideCharacterEquip();
    private void OnVmCloseDestroyRequested(object? sender) => HideCharacterEquip(true);

    private void OnVmSelectedReadyRequested(object? sender)
    {
        if (_teamInventory != null && _teamInventory.IsEnabled)
        {
            _teamInventory.RequestSpawnWithTeamEquipment();
            HideCharacterEquip();
        }

        if (_userLoadout != null && _userLoadout.IsEnabled)
        {
            _userLoadout.RequestSetSpawnReady();
            _characterEquipVm?.IsSpawnButtonVisible = false;
            HideCharacterEquip();
        }
    }

    private void ToggleCharacterEquip()
    {
        if (_characterEquipVm == null || !_characterEquipVm.IsVisible)
        {
            OpenCharacterEquip();
        }
        else
        {
            HideCharacterEquip();
        }
    }

    private void OpenCharacterEquip()
    {
        if (_userLoadout == null || !_userLoadout.IsEnabled)
        {
            return;
        }

        if (_lobbyComponent?.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.Ending)
        {
            return;
        }

        if (_teamInventory?.IsEnabled == true) // disable showing gui if teaminventory enabled and hasnt picked a side
        {
            if (GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.Team?.Side == TaleWorlds.Core.BattleSideEnum.None)
            {
                return;
            }
        }

        if (_characterEquipLayer == null)
        {
            _characterEquipVm = new CrpgCharacterEquipVM();
            _characterEquipVm.OnCloseButtonClicked += _onVmCloseHandler;
            _characterEquipVm.OnCloseButtonAlternateClicked += _onVmCloseDestoryHandler;
            _characterEquipVm.OnSelectedReadyClicked += _onVmSelectedReadyHandler;

            _characterEquipLayer = new GauntletLayer("CrpgCharacterEquipPrefab", 110) { IsFocusLayer = true };
            _characterEquipLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _characterEquipLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            _characterEquipMovieId = _characterEquipLayer.LoadMovie("CrpgCharacterEquipPrefab", _characterEquipVm);

            if (ScreenManager.TopScreen is MissionScreen missionScreen)
            {
                missionScreen.AddLayer(_characterEquipLayer);
            }
            else
            {
                ScreenManager.TopScreen?.AddLayer(_characterEquipLayer);
            }
        }

        if (_firstOpen)
        {
            _userLoadout?.RequestGetUserInfo();
            _userLoadout?.RequestGetUpdatedCharacterBasic();
            _firstOpen = false;
        }

        _characterEquipVm!.IsVisible = true;
        _characterEquipLayer.IsFocusLayer = true;
        _characterEquipLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
        ScreenManager.TrySetFocus(_characterEquipLayer);
        SoundEvent.PlaySound2D("event:/ui/panels/panel_inventory_open");
        _characterEquipVm.RefreshSpawnButtonVisibility();

        var now = DateTime.UtcNow;
        if (now - _lastApiRefreshTime >= ApiRefreshCooldown)
        {
            _lastApiRefreshTime = now;
            _userLoadout?.RequestGetUserInfo();
            _userLoadout?.RequestGetUpdatedCharacterBasic();

            if (_teamInventory?.IsEnabled == true)
            {
                // _teamInventory.RequestGetTeamItemsList();
            }
            else
            {
                _userLoadout?.RequestGetUserInventoryItems();
                _userLoadout?.RequestGetUpdatedCharacterEquippedItems();
                _clanArmory?.RequestArmoryAction(ClanArmoryActionType.Get, null);
            }
        }
    }

    private void HideCharacterEquip(bool destroy = false)
    {
        if (_characterEquipLayer == null || _characterEquipVm == null)
        {
            return;
        }

        _characterEquipLayer.IsFocusLayer = false;
        _characterEquipLayer.InputRestrictions.ResetInputRestrictions();
        ScreenManager.TryLoseFocus(_characterEquipLayer);

        if (destroy)
        {
            _characterEquipVm.OnCloseButtonClicked -= _onVmCloseHandler;
            _characterEquipVm.OnSelectedReadyClicked -= _onVmSelectedReadyHandler;
            _characterEquipVm.OnFinalize();
            _characterEquipVm = null;

            _lastApiRefreshTime = DateTime.MinValue;
            _firstOpen = true;

            if (_characterEquipMovieId != null)
            {
                _characterEquipLayer.ReleaseMovie(_characterEquipMovieId);
                _characterEquipMovieId = null;
            }

            if (ScreenManager.TopScreen is MissionScreen missionScreen)
            {
                missionScreen.RemoveLayer(_characterEquipLayer);
            }
            else
            {
                ScreenManager.TopScreen?.RemoveLayer(_characterEquipLayer);
            }

            _characterEquipLayer = null;
        }
        else
        {
            _characterEquipVm.IsVisible = false;
        }
    }

    private void HandleUserLoadoutEnabledChanged(bool isEnabled)
    {
        if (_characterEquipVm != null)
        {
            bool wasVisible = _characterEquipVm.IsVisible;
            HideCharacterEquip(destroy: true);
        }
    }

    private void HandleTeamInventoryEnabledChanged(bool isEnabled)
    {
        // destroy and recreate the VM with the new enabled state
        if (_characterEquipVm != null)
        {
            bool wasVisible = _characterEquipVm.IsVisible;
            HideCharacterEquip(destroy: true);
            if (wasVisible)
            {
                OpenCharacterEquip();
            }
        }
    }

    private void HandleForceEquipMenu(bool show, bool showReadyButton, string msg)
    {
        if (show)
        {
            if (_characterEquipVm?.IsVisible != true)
            {
                OpenCharacterEquip();
            }

            _characterEquipVm?.SelectNavBarTab(NavBarTab.Equipment);
            CrpgCharacterEquipVM.RequestStatusMessage(msg, true, 10);
        }
        else
        {
            HideCharacterEquip();
        }
    }

    private void HandlePostMatchEnded()
    {
        HideCharacterEquip(destroy: true);
    }
}

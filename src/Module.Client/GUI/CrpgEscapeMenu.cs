using Crpg.Module.GUI.Inventory;
using Crpg.Module.Modes.Duel;
using Crpg.Module.Modes.TrainingGround;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.PlatformService;

namespace Crpg.Module.GUI;

// We need to use the OverrideView here since the CrpgEscapeMenu constructor requires a parameter.
[OverrideView(typeof(CrpgMissionMultiplayerEscapeMenu))]
internal class CrpgEscapeMenu : MissionGauntletMultiplayerEscapeMenu
{
    private const string CrpgWebsite = "https://c-rpg.eu/characters/";
    private readonly MissionMultiplayerGameModeBaseClient _gameModeClient;

    public CrpgEscapeMenu(string gameMode, MissionMultiplayerGameModeBaseClient gameModeClient)
        : base(gameMode)
    {
        _gameModeClient = gameModeClient;
    }

    protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
    {
        List<EscapeMenuItemVM> items = base.GetEscapeMenuItems();
        EscapeMenuItemVM crpgWebsiteButton = new(new TextObject("{=FAkcpZdy}Character & Shop"),
            __ => _ = PlatformServices.Instance.ShowOverlayForWebPage(CrpgWebsite),
            null, () => Tuple.Create(false, new TextObject(string.Empty)));

        if (_gameModeClient is CrpgDuelMissionMultiplayerClient)
        {
            AddDuelModeOptions(items);
        }
        else if (_gameModeClient is CrpgTrainingGroundMissionMultiplayerClient)
        {
            AddTrainingGroundOptions(items);
        }

        EscapeMenuItemVM inventoryButton = new(new TextObject("{=}Inventory"),
            __ =>
            {
                var inventoryScreen = Mission.GetMissionBehavior<CrpgInventoryScreen>();
                inventoryScreen?.ToggleVisibility();
                OnEscapeMenuToggled(false);
            },
            null, () => Tuple.Create(false, new TextObject(string.Empty)));

        items.Insert(items.Count - 2, crpgWebsiteButton); // -2 = Insert new button right before the 'Options' button
        items.Insert(items.Count - 2, inventoryButton);

        return items;
    }

    private void AddDuelModeOptions(List<EscapeMenuItemVM> items)
    {
        MissionPeer component = GameNetwork.MyPeer.GetComponent<MissionPeer>();

        if (component == null || component.Team == null || component.Representative is not DuelMissionRepresentative)
        {
            return;
        }

        var duelBehavior = Mission.GetMissionBehavior<MissionMultiplayerGameModeDuelClient>();
        if (duelBehavior?.IsInDuel ?? false)
        {
            return;
        }

        EscapeMenuItemVM preferredArenaInfButton = new(new TextObject("Arena: Infantry"), _ =>
        {
            DuelModeChangeArena(TroopType.Infantry);
            OnEscapeMenuToggled(false);
        }, null, () => Tuple.Create(false, new TextObject(string.Empty)));

        EscapeMenuItemVM preferredArenaArcButton = new(new TextObject("Arena: Ranged"), _ =>
        {
            DuelModeChangeArena(TroopType.Ranged);
            OnEscapeMenuToggled(false);
        }, null, () => Tuple.Create(false, new TextObject(string.Empty)));

        EscapeMenuItemVM preferredArenaCavButton = new(new TextObject("Arena: Cavalry"), _ =>
        {
            DuelModeChangeArena(TroopType.Cavalry);
            OnEscapeMenuToggled(false);
        }, null, () => Tuple.Create(false, new TextObject(string.Empty)));

        List<EscapeMenuItemVM> newButtons = new() { preferredArenaInfButton, preferredArenaArcButton, preferredArenaCavButton };
        items.InsertRange(items.Count - 2, newButtons);
    }

    private void AddTrainingGroundOptions(List<EscapeMenuItemVM> items)
    {
        MissionPeer component = GameNetwork.MyPeer.GetComponent<MissionPeer>();

        if (component == null || component.Team == null || component.Representative is not CrpgTrainingGroundMissionRepresentative)
        {
            return;
        }

        var duelBehavior = Mission.GetMissionBehavior<CrpgTrainingGroundMissionMultiplayerClient>();
        if (duelBehavior?.IsInDuel ?? false)
        {
            return;
        }

        EscapeMenuItemVM preferredArenaInfButton = new(new TextObject("{=}Refresh Character"), _ =>
        {
            RefreshCharacter();
            OnEscapeMenuToggled(false);
        }, null, () => Tuple.Create(false, new TextObject(string.Empty)));

        List<EscapeMenuItemVM> newButtons = new() { preferredArenaInfButton };
        items.InsertRange(items.Count - 2, newButtons);
    }

    private void DuelModeChangeArena(TroopType troopType)
    {
        MissionPeer component = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        if (component == null || component.Team == null || component.Representative is not DuelMissionRepresentative duelRepresentative)
        {
            return;
        }

        if (component.Team.IsDefender)
        {
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=v5EqMSlD}Can't change arena preference while in duel.").ToString()));
            return;
        }

        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new RequestChangePreferredTroopType(troopType));
        GameNetwork.EndModuleEventAsClient();
        duelRepresentative.OnMyPreferredZoneChanged?.Invoke(troopType);
    }

    private void RefreshCharacter()
    {
        MissionPeer component = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        if (component == null || component.Team == null || component.Representative is not CrpgTrainingGroundMissionRepresentative duelRepresentative)
        {
            return;
        }

        if (component.Team.IsDefender)
        {
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=}Can't refresh character while you are in a duel.").ToString()));
            return;
        }

        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new ClientRequestRefreshCharacter());
        GameNetwork.EndModuleEventAsClient();
    }
}

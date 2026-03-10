using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common.Commander;
internal class CrpgCommanderBehaviorClient : MissionNetwork
{
    private static readonly string[] CommanderSuicideStrings =
    {
        "{=1qdkTUv7}The Commander, {COMMANDER} has died!",
        "{=Opm24MMd}Commander {COMMANDER} managed to kill themself... somehow.",
        "{=s894oFCj}Commander {COMMANDER} died, spectacularly.",
        "{=vu94iRTE}Commander {COMMANDER} has fallen! ",
        "{=sy9OJEOM}Commander {COMMANDER} didn't stand a chance!",
    };

    private static readonly string[] CommanderTeamKilledStrings =
    {
        "{=uD9I4mVt}Commander {COMMANDER} has been team-killed by {AGENT}! An honest mistake!",
        "{=IXqXzFNt}Commander {COMMANDER} has been team-killed by {AGENT}! Surely an accident!",
        "{=7lJdMRdX}{AGENT} has mutinied, killing Commander {COMMANDER}!",
        "{=K14mdvO5}{AGENT} should be more careful! They killed Commander {COMMANDER}!",
        "{=NcM64S4W}Commander {COMMANDER} has been struck down by their ally, {AGENT}!",
    };

    private static readonly string[] CommanderKilledStrings =
    {
        "{=8a5Icfba}The Commander, {COMMANDER} has died!",
        "{=wBRZQfd6}Commander {COMMANDER} has been killed by {AGENT}!",
        "{=sEkwjLWt}{AGENT} has killed {COMMANDER}, The Commander!",
        "{=ZnefgOjm}Commander {COMMANDER} has been vanquished by {AGENT}, a fine display!",
        "{=cUdNADKW}Commander {COMMANDER} didn't stand a chance! {AGENT} made sure of that.",
        "{=X41TEG4i}{AGENT} defeated Commander {COMMANDER} in fair combat!",
        "{=ITo28ACb}{AGENT} has killed Commander {COMMANDER} in the heat of battle!",
    };

    private Dictionary<BattleSideEnum, NetworkCommunicator?> _commanders = new();
    private Dictionary<BattleSideEnum, BasicCharacterObject?> _commanderCharacters = new();

    public event Action<BattleSideEnum> OnCommanderUpdated = null!;

    public CrpgCommanderBehaviorClient()
    {
        _commanders[BattleSideEnum.Attacker] = null;
        _commanders[BattleSideEnum.Defender] = null;
        _commanders[BattleSideEnum.None] = null;

        _commanderCharacters[BattleSideEnum.Attacker] = null;
        _commanderCharacters[BattleSideEnum.Defender] = null;
        _commanderCharacters[BattleSideEnum.None] = null;
    }

    public NetworkCommunicator? GetCommanderBySide(BattleSideEnum side)
    {
        return _commanders[side];
    }

    public BasicCharacterObject? GetCommanderCharacterObjectBySide(BattleSideEnum side)
    {
        return _commanderCharacters[side];
    }

    public bool IsPeerCommander(MissionPeer peer)
    {
        NetworkCommunicator? networkCommunicator = peer.GetNetworkPeer();
        if (networkCommunicator == null)
        {
            return false;
        }

        return _commanders.Any(kvp => kvp.Value == networkCommunicator);
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<UpdateCommander>(HandleUpdateCommander);
        registerer.Register<CommanderKilled>(HandleCommanderKilled);
        registerer.Register<CommanderChatCommand>(HandleCommanderChatCommand);
    }

    private void HandleCommanderChatCommand(CommanderChatCommand message)
    {
        TextObject textObject;

        if (message.RejectReason == CommanderChatCommandRejectReason.NotCommander)
        {
            textObject = new("{=7WDWLbpV}You are not the Commander!");
        }
        else if (message.RejectReason == CommanderChatCommandRejectReason.Dead)
        {
            textObject = new("{=C7TYZ8s0}You cannot order troops when you are dead!");
        }
        else if (message.RejectReason == CommanderChatCommandRejectReason.Muted)
        {
            textObject = new("{=sisK3HPI}You are muted.");
        }
        else if (message.RejectReason == CommanderChatCommandRejectReason.TargetIsMuted)
        {
            textObject = new("{=aB5Qq8bC}The target player is muted.");
        }
        else
        {
            textObject = new("{=uRmpZM0q}Please wait {COOLDOWN} seconds before issuing a new order!",
            new Dictionary<string, object> { ["COOLDOWN"] = message.Cooldown.ToString("0.0") });
        }

        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = textObject.ToString(),
            Color = Color.White,
            SoundEventPath = "event:/ui/item_close",
        });
    }

    private void HandleUpdateCommander(UpdateCommander message)
    {
        BattleSideEnum mySide = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.Team?.Side ?? BattleSideEnum.None;
        _commanders[message.Side] = message.Commander;
        _commanderCharacters[message.Side] = BuildCommanderCharacterObject(message.Side);
        TextObject textObject;
        Color color;

        if (mySide != BattleSideEnum.None)
        {
            if (message.Commander != null)
            {
                textObject = new("{=FnpBBZ95}{SIDE} have promoted {COMMANDER} to be their commander!",
                new Dictionary<string, object> { ["SIDE"] = message.Side == mySide ? new TextObject("{=pFNbCPS7}Your team").ToString() : new TextObject("{=uucWY8gP}The enemy team"), ["COMMANDER"] = message.Commander.UserName });
                color = message.Side == mySide ? new(0.1f, 1f, 0f) : new(0.90f, 0.25f, 0.25f);
            }
            else
            {
                textObject = new("{=IvlgRIsN}{SIDE} commander has resigned!",
                new Dictionary<string, object> { ["SIDE"] = message.Side == mySide ? new TextObject("{=vq5Mzgl9}Your").ToString() : new TextObject("{=0BZxL0rw}The enemy") });
                color = message.Side == mySide ? new(0.90f, 0.25f, 0.25f) : new(0.1f, 1f, 0f);
            }

            InformationManager.DisplayMessage(new InformationMessage
            {
                Information = textObject.ToString(),
                Color = color,
                SoundEventPath = "event:/ui/notification/war_declared",
            });
        }

        OnCommanderUpdated?.Invoke(message.Side);
    }

    private void HandleCommanderKilled(CommanderKilled message)
    {
        var killerAgent = Mission.MissionNetworkHelper.GetAgentFromIndex(message.AgentKillerIndex, true);
        var commanderAgent = Mission.MissionNetworkHelper.GetAgentFromIndex(message.AgentCommanderIndex, true);

        if (commanderAgent == null)
        {
            return;
        }

        BattleSideEnum commanderSide = commanderAgent.MissionPeer?.Team?.Side ?? BattleSideEnum.None;
        BattleSideEnum killerSide = killerAgent?.MissionPeer?.Team?.Side ?? BattleSideEnum.None;
        BattleSideEnum mySide = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.Team?.Side ?? BattleSideEnum.None;

        TextObject textObject;

        if (message.AgentKillerIndex == message.AgentCommanderIndex)
        {
            textObject = new(CommanderSuicideStrings.GetRandomElement(),
            new Dictionary<string, object> { ["COMMANDER"] = commanderAgent.Name });
        }
        else if (commanderSide == killerSide)
        {
            textObject = new(CommanderTeamKilledStrings.GetRandomElement(),
            new Dictionary<string, object> { ["AGENT"] = killerAgent?.Name ?? string.Empty, ["COMMANDER"] = commanderAgent.Name });
        }
        else
        {
            textObject = new(CommanderKilledStrings.GetRandomElement(),
            new Dictionary<string, object> { ["AGENT"] = killerAgent?.Name ?? string.Empty, ["COMMANDER"] = commanderAgent.Name });
        }

        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = textObject.ToString(),
            Color = commanderSide == mySide ? new Color(0.90f, 0.25f, 0.25f) : new Color(0.1f, 1f, 0f),
            SoundEventPath = commanderSide == mySide ? "event:/ui/mission/multiplayer/pointlost" : "event:/ui/mission/multiplayer/pointcapture",
        });
    }

    private BasicCharacterObject? BuildCommanderCharacterObject(BattleSideEnum side)
    {
        if (_commanders[side] != null)
        {
            MissionPeer missionPeer = _commanders[side].GetComponent<MissionPeer>();
            string character = side == BattleSideEnum.Attacker ? "crpg_commander_attacker" : "crpg_commander_defender";
            BasicCharacterObject commanderCharacterObject = MBObjectManager.Instance.GetObject<BasicCharacterObject>(character);
            commanderCharacterObject.UpdatePlayerCharacterBodyProperties(missionPeer.Peer.BodyProperties, missionPeer.Peer.Race, missionPeer.Peer.IsFemale);
            commanderCharacterObject.Age = missionPeer.Peer.BodyProperties.Age;
            commanderCharacterObject.GetName();

            var crpgUser = missionPeer.Peer.GetComponent<CrpgPeer>()?.User;

            if (crpgUser != null)
            {
                var equipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgUser.Character.EquippedItems);
                MBEquipmentRoster equipmentRoster = new();
                ReflectionHelper.SetField(equipmentRoster, "_equipments", new MBList<Equipment> { equipment });
                ReflectionHelper.SetField(commanderCharacterObject, "_equipmentRoster", equipmentRoster);
                ReflectionHelper.SetField(commanderCharacterObject, "_basicName", new TextObject(missionPeer.Name));
            }

            return commanderCharacterObject;
        }

        return null;
    }
}

using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.TrainingGround;

internal class CrpgTrainingGroundSpawningBehavior : CrpgSpawningBehaviorBase
{
    private readonly CrpgTrainingGroundServer _server;

    public CrpgTrainingGroundSpawningBehavior(CrpgConstants constants, CrpgTrainingGroundServer server)
        : base(constants)
    {
        IsSpawningEnabled = true;
        _server = server;
        CurrentGameMode = MultiplayerGameType.Duel;
    }

    public override void OnTick(float dt)
    {
        if (IsSpawningEnabled && SpawnCheckTimer.Check(Mission.CurrentTime))
        {
            SpawnAgents();
        }

        base.OnTick(dt);
    }

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
        return missionPeer.Culture != null
               && missionPeer.Representative is CrpgTrainingGroundMissionRepresentative
               && missionPeer.SpawnTimer.Check(Mission.CurrentTime);
    }

    protected override bool IsRoundInProgress()
    {
        return Mission.CurrentState == Mission.State.Continuing;
    }

    protected override void OnPeerSpawned(Agent agent)
    {
        base.OnPeerSpawned(agent);
        _ = agent.MissionPeer.Representative; // Get initializes the representative

        var networkPeer = agent.MissionPeer?.GetNetworkPeer();
        if (networkPeer == null)
        {
            return;
        }
    }

    public bool RefreshPlayer(NetworkCommunicator networkPeer)
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());

        MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
        CrpgPeer crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        Agent controlledAgent = missionPeer.ControlledAgent;

        if (!networkPeer.IsSynchronized
            || missionPeer == null
            || controlledAgent == null
            || missionPeer.Team == null
            || missionPeer.Team == Mission.SpectatorTeam
            || crpgPeer == null
            || crpgPeer.UserLoading
            || crpgPeer.User == null
            || !IsPlayerAllowedToSpawn(networkPeer))
        {
            return false;
        }

        controlledAgent.ClearEquipment();
        controlledAgent.FadeOut(true, true);

        BasicCultureObject teamCulture = missionPeer.Team == Mission.AttackerTeam ? cultureTeam1 : cultureTeam2;
        var peerClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("crpg_captain_division_1");
        var characterSkills = CrpgCharacterBuilder.CreateCharacterSkills(crpgPeer.User!.Character.Characteristics);
        var characterXml = peerClass.HeroCharacter;

        var characterEquipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);

        MatrixFrame spawnFrame = controlledAgent.Frame;
        var troopOrigin = new CrpgBattleAgentOrigin(characterXml, characterSkills);
        CrpgCharacterBuilder.AssignArmorsToTroopOrigin(troopOrigin, crpgPeer.User.Character.EquippedItems.ToList());
        AgentBuildData agentBuildData = new AgentBuildData(characterXml)
            .MissionPeer(missionPeer)
            .Equipment(characterEquipment)
            .TroopOrigin(troopOrigin)
            .Team(missionPeer.Team)
            .VisualsIndex(0)
            .IsFemale(missionPeer.Peer.IsFemale)
            .BodyProperties(characterXml.GetBodyPropertiesMin())
            .InitialPosition(spawnFrame.origin)
            .InitialDirection(spawnFrame.rotation.f.AsVec2);

        if (crpgPeer.Clan != null)
        {
            agentBuildData.ClothingColor1(crpgPeer.Clan.PrimaryColor);
            agentBuildData.ClothingColor2(crpgPeer.Clan.SecondaryColor);
            if (!string.IsNullOrEmpty(crpgPeer.Clan.BannerKey))
            {
                agentBuildData.Banner(new Banner(crpgPeer.Clan.BannerKey));
            }
        }
        else
        {
            agentBuildData.ClothingColor1(missionPeer.Team == Mission.AttackerTeam
                ? teamCulture.Color
                : teamCulture.ClothAlternativeColor);
            agentBuildData.ClothingColor2(missionPeer.Team == Mission.AttackerTeam
                ? teamCulture.Color2
                : teamCulture.ClothAlternativeColor2);
        }

        Agent agent = Mission.SpawnAgent(agentBuildData);
        OnPeerSpawned(agent);
        CrpgAgentComponent agentComponent = new(agent);
        agent.AddComponent(agentComponent);

        bool hasExtraSlotEquipped = characterEquipment[EquipmentIndex.ExtraWeaponSlot].Item != null;
        if (!agent.HasMount || hasExtraSlotEquipped)
        {
            agent.WieldInitialWeapons();
        }

        return true;
    }
}

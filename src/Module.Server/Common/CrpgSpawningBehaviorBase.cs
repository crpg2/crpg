using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.Common;

internal abstract class CrpgSpawningBehaviorBase : SpawningBehaviorBase
{
    protected static float TimeSinceSpawnEnabled { get; set; }
    private readonly CrpgConstants _constants;

    private readonly List<WeaponClass> _allowedSpawnWeaponClass =
    [
        WeaponClass.Dagger,
        WeaponClass.Mace,
        WeaponClass.TwoHandedMace,
        WeaponClass.OneHandedSword,
        WeaponClass.TwoHandedSword,
        WeaponClass.OneHandedAxe,
        WeaponClass.TwoHandedAxe,
        WeaponClass.Pick,
        WeaponClass.LowGripPolearm,
        WeaponClass.OneHandedPolearm,
        WeaponClass.TwoHandedPolearm,
        WeaponClass.Javelin,
        WeaponClass.Stone,
        WeaponClass.ThrowingAxe,
        WeaponClass.ThrowingKnife
    ];

    private readonly Dictionary<Team, int> _teamSumOfEquipment = new();
    private readonly Dictionary<Team, int> _teamAverageEquipment = new();
    private readonly Dictionary<Team, int> _teamNumberOfBots = new();
#if CRPG_SERVER
    private readonly int _totalNumberOfBots = CrpgServerConfiguration.ControlledBotsCount;
#else
    private readonly int _totalNumberOfBots = 800;
#endif

    public CrpgSpawningBehaviorBase(CrpgConstants constants)
    {
        _constants = constants;
    }

    protected MultiplayerGameType CurrentGameMode { get; set; } = MultiplayerGameType.Battle;

    public float TimeUntilRespawn(Team team)
    {
        int respawnPeriod = team.Side == BattleSideEnum.Defender
        ? MultiplayerOptions.OptionType.RespawnPeriodTeam2.GetIntValue()
        : MultiplayerOptions.OptionType.RespawnPeriodTeam1.GetIntValue();
        float timeSinceLastRespawn = TimeSinceSpawnEnabled % respawnPeriod;
        float timeUntilNextRespawn = respawnPeriod - timeSinceLastRespawn;

        if (timeUntilNextRespawn <= 1.0f)
        {
            timeUntilNextRespawn = 0f;
        }

        return timeUntilNextRespawn;
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        return false;
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        base.OnAllAgentsFromPeerSpawnedFromVisuals += OnAllAgentsFromPeerSpawnedFromVisuals;
    }

    public override void RequestStartSpawnSession()
    {
        foreach (Team team in Mission.Current.Teams)
        {
            _teamSumOfEquipment[team] = ComputeTeamSumOfEquipmentValue(team);
            _teamAverageEquipment[team] = ComputeTeamAverageUnitValue(team, _teamSumOfEquipment[team]);
        }

        foreach (Team team in Mission.Current.Teams)
        {
            int teamCount = GameNetwork.NetworkPeers.Count(p => IsNetworkPeerRelevant(p) && p.GetComponent<MissionPeer>().Team == team);
            float numerator = _totalNumberOfBots * _teamAverageEquipment.Where(kvp => kvp.Key != team).Sum(kvp => kvp.Value);
            float denominator = (Mission.Current.Teams.Count - 2) * _teamAverageEquipment.Sum(kvp => kvp.Value); // -2 because we also remove spectator
            if (teamCount <= 1)
            {
                _teamNumberOfBots[team] = _totalNumberOfBots / (Mission.Current.Teams.Count - 1);
            }
            else
            {
                _teamNumberOfBots[team] = (int)(numerator / denominator);
            }
        }

        base.RequestStartSpawnSession();
        ResetSpawnTeams();
    }

    protected virtual bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        return true;
    }

    protected virtual bool IsBotTeamAllowedToSpawn(Team team)
    {
        return true;
    }

    protected override void SpawnAgents()
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        int heroClassId = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
            CrpgPeer crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (!networkPeer.IsSynchronized
                || missionPeer == null
                || missionPeer.ControlledAgent != null
                || missionPeer.Team == null
                || missionPeer.Team == Mission.SpectatorTeam
                || crpgPeer == null
                || crpgPeer.UserLoading
                || crpgPeer.User == null
                || !IsPlayerAllowedToSpawn(networkPeer))
            {
                continue;
            }

            heroClassId++;
            BasicCultureObject teamCulture = missionPeer.Team == Mission.AttackerTeam ? cultureTeam1 : cultureTeam2;
            var peerClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>($"crpg_captain_division_{heroClassId}");
            // var character = CreateCharacter(crpgPeer.User.Character, _constants);
            var characterSkills = CrpgCharacterBuilder.CreateCharacterSkills(crpgPeer.User!.Character.Characteristics);
            var characterXml = peerClass.HeroCharacter;

            var characterEquipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);
            /* CrpgCharacterObject character = CreateCharacter(crpgPeer.User.Character, _constants, characterSkills, characterEquipment);
             * is still not possible. Characters are tightly coupled to xmls. The code does things like

                *MBObjectManager.Instance.GetObjectTypeList<MultiplayerClassDivisions.MPHeroClass>().FirstOrDefau
                  ((MultiplayerClassDivisions.MPHeroClass x) => x.HeroCharacter == character || x.TroopCharacter == character);
            Expecting that a character always exist in xmls
             */

            bool hasMount = characterEquipment[EquipmentIndex.Horse].Item != null;

            bool firstSpawn = missionPeer.SpawnCountThisRound == 0;
            MatrixFrame spawnFrame = SpawnComponent.GetSpawnFrame(missionPeer.Team, hasMount, firstSpawn);
            Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();
            // Randomize direction so players don't go all straight.
            initialDirection.RotateCCW(MBRandom.RandomFloatRanged(-MathF.PI / 3f, MathF.PI / 3f));
            var troopOrigin = new CrpgBattleAgentOrigin(characterXml, characterSkills);
            CrpgCharacterBuilder.AssignArmorsToTroopOrigin(troopOrigin, crpgPeer.User.Character.EquippedItems.ToList());
            Formation? formation = missionPeer.ControlledFormation;
            if (formation == null)
            {
                formation = missionPeer.Team.FormationsIncludingEmpty.FirstOrDefault(x => x.PlayerOwner == null && x.CountOfUnits == 0);
                if (formation != null)
                {
                    formation.ContainsAgentVisuals = true;
                    if (string.IsNullOrEmpty(formation.BannerCode))
                    {
                        formation.BannerCode = missionPeer.Peer.BannerCode;
                    }
                }
            }

            missionPeer.ControlledFormation = formation;
            missionPeer.HasSpawnedAgentVisuals = true;
            AgentBuildData agentBuildData = new AgentBuildData(characterXml)
                .MissionPeer(missionPeer)
                .Equipment(characterEquipment)
                .TroopOrigin(troopOrigin)
                .Team(missionPeer.Team)
                .VisualsIndex(0)
                .IsFemale(missionPeer.Peer.IsFemale)
                // base.GetBodyProperties uses the player-defined body properties but some body properties may have been
                // causing crashes. So here we send the body properties from the characters.xml which we know are safe.
                // Note that what is sent here doesn't matter since it's ignored by the client.
                .BodyProperties(characterXml.GetBodyPropertiesMin())
                .InitialPosition(in spawnFrame.origin)
                .InitialDirection(in initialDirection)
                .Formation(formation);

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

            if (IsRoundInProgress() && (CurrentGameMode == MultiplayerGameType.Captain || CurrentGameMode == MultiplayerGameType.Battle))
            {
                List<NetworkCommunicator> peers = GameNetwork.NetworkPeers;
                List<NetworkCommunicator> teamRelevantPeers =
                    peers.Where(p => IsNetworkPeerRelevant(p) && p.GetComponent<MissionPeer>().Team == missionPeer.Team).ToList();
                float sumOfTeamEquipment = _teamSumOfEquipment[missionPeer.Team];
                float peerSumOfEquipment = ComputeEquipmentValue(crpgPeer);
                int peerNumberOfBots;
                if (teamRelevantPeers.Count - 1 < 1)
                {
                    peerNumberOfBots = _teamNumberOfBots[missionPeer.Team];
                }
                else
                {
                    peerNumberOfBots = (int)(_teamNumberOfBots[missionPeer.Team] * (1 - peerSumOfEquipment / sumOfTeamEquipment) /
                             (teamRelevantPeers.Count - 1));
                }

                for (int i = 0; i < peerNumberOfBots; i++)
                {
                    SpawnBotAgent($"crpg_captain_bot_division_{heroClassId}", agent.Team, missionPeer, heroClassId);
                }
            }

            // AgentVisualSpawnComponent.RemoveAgentVisuals(missionPeer, sync: true);
        }
    }

    protected Agent SpawnBotAgent(string classDivisionId, Team team, MissionPeer? peer = null, int p = 0, bool isInitialSpawn = true)
    {
        var teamCulture = team.Side == BattleSideEnum.Attacker
            ? MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue())
            : MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());

        MultiplayerClassDivisions.MPHeroClass botClass = MultiplayerClassDivisions
            .GetMPHeroClasses()
        .First(h => h.StringId == classDivisionId);
        BasicCharacterObject character = botClass.HeroCharacter;

        bool hasMount = character.Equipment[EquipmentIndex.Horse].Item != null;
        MatrixFrame spawnFrame = SpawnComponent.GetSpawnFrame(team, hasMount, isInitialSpawn);
        Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();

        AgentBuildData agentBuildData = new AgentBuildData(character)
            .Equipment(character.RandomBattleEquipment)
            .TroopOrigin(new BasicBattleAgentOrigin(character))
            .EquipmentSeed(MissionLobbyComponent.GetRandomFaceSeedForCharacter(character))
            .Team(team)
            .VisualsIndex(0)
            .InitialPosition(in spawnFrame.origin)
            .InitialDirection(in initialDirection)
            .IsFemale(character.IsFemale)
            .ClothingColor1(
                team.Side == BattleSideEnum.Attacker ? teamCulture.Color : teamCulture.ClothAlternativeColor)
            .ClothingColor2(team.Side == BattleSideEnum.Attacker
                ? teamCulture.Color2
                : teamCulture.ClothAlternativeColor2);
        if (peer != null)
        {
            agentBuildData.OwningMissionPeer(peer);
            agentBuildData.Formation(peer.ControlledFormation);
            var crpgPeer = peer.GetComponent<CrpgPeer>();
            if (crpgPeer != null && crpgPeer.User != null)
            {
                Equipment characterEquipment = CrpgCharacterBuilder.CreateBotCharacterEquipment(crpgPeer.User.Character.EquippedItems);
                MultiplayerClassDivisions.MPHeroClass? peerClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>($"crpg_captain_bot_division_{p}");
                MBCharacterSkills characterSkills = CrpgCharacterBuilder.CreateCharacterSkills(crpgPeer.User!.Character.Characteristics);
                BasicCharacterObject? characterXml = peerClass.HeroCharacter;
                CrpgBattleAgentOrigin troopOrigin = new(characterXml, characterSkills);
                agentBuildData.OwningMissionPeer(peer);
                agentBuildData.Formation(peer.ControlledFormation);
                agentBuildData.Equipment(characterEquipment);
                agentBuildData.TroopOrigin(troopOrigin);
                agentBuildData.Banner(new Banner(peer.Peer.BannerCode));
            }
        }

        var bodyProperties = BodyProperties.GetRandomBodyProperties(
            character.Race,
            character.IsFemale,
            character.GetBodyPropertiesMin(),
            character.GetBodyPropertiesMax(),
            (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType,
            agentBuildData.AgentEquipmentSeed,
            character.BodyPropertyRange.HairTags,
            character.BodyPropertyRange.BeardTags,
            character.BodyPropertyRange.TattooTags);
        agentBuildData.BodyProperties(bodyProperties);

        Agent agent = Mission.SpawnAgent(agentBuildData);
#if CRPG_SERVER
        if (!CrpgServerConfiguration.FrozenBots)
        {
            agent.SetWatchState(Agent.WatchState.Alarmed);
        }
#endif
        agent.WieldInitialWeapons();
        return agent;
    }

    protected void SpawnBotAgents(bool isInitialSpawn = true)
    {
        int botsTeam1 = MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue();
        int botsTeam2 = MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue();

        if (CurrentGameMode == MultiplayerGameType.Battle || CurrentGameMode == MultiplayerGameType.Captain)
        {
            int team1Count = GameNetwork.NetworkPeers.Where(p => IsNetworkPeerRelevant(p) && p.GetComponent<MissionPeer>().Team.Side == BattleSideEnum.Attacker).ToList().Count;
            int team2Count = GameNetwork.NetworkPeers.Where(p => IsNetworkPeerRelevant(p) && p.GetComponent<MissionPeer>().Team.Side == BattleSideEnum.Defender).ToList().Count;
            if (team1Count != 0)
            {
                botsTeam1 = 0;
            }
            else
            {
                botsTeam1 = _totalNumberOfBots / (Mission.Current.Teams.Count - 1);
            }

            if (team2Count != 0)
            {
                botsTeam2 = 0;
            }
            else
            {
                botsTeam2 = _totalNumberOfBots / (Mission.Current.Teams.Count - 1);
            }
        }

        if (botsTeam1 <= 0 && botsTeam2 <= 0)
        {
            return;
        }

        Mission.Current.AllowAiTicking = false;
        foreach (Team team in Mission.Teams)
        {
            if (Mission.AttackerTeam != team && Mission.DefenderTeam != team)
            {
                continue;
            }

            if (!IsBotTeamAllowedToSpawn(team))
            {
                continue;
            }

            int numberOfBots = team.Side == BattleSideEnum.Attacker
                ? botsTeam1
                : botsTeam2;
            int numberOfPlayers = GameNetwork.NetworkPeers.Count(p => p.IsSynchronized && p.GetComponent<MissionPeer>()?.Team == team);
            int botsAlive = team.ActiveAgents.Count(a => a.IsAIControlled && a.IsHuman);

            for (int i = 0 + botsAlive + numberOfPlayers; i < numberOfBots; i += 1)
            {
                MultiplayerClassDivisions.MPHeroClass botClass = MultiplayerClassDivisions
                    .GetMPHeroClasses()
                    .GetRandomElementWithPredicate<MultiplayerClassDivisions.MPHeroClass>(x => x.StringId.StartsWith("crpg_bot_"));
                SpawnBotAgent(botClass.StringId, team, isInitialSpawn: isInitialSpawn);
            }
        }
    }

    protected virtual void OnPeerSpawned(Agent agent)
    {
        if (agent.MissionPeer.ControlledFormation != null)
        {
            agent.Team.AssignPlayerAsSergeantOfFormation(agent.MissionPeer,
                agent.MissionPeer.ControlledFormation.FormationIndex);
        }

        CrpgPeer? crpgPeer = agent.MissionPeer.GetComponent<CrpgPeer>();
        crpgPeer.LastSpawnInfo = new SpawnInfo(agent.MissionPeer.Team, crpgPeer.User!.Character.EquippedItems);
    }

    protected bool DoesEquipmentContainWeapon(Equipment equipment)
    {
        for (var i = EquipmentIndex.Weapon0; i <= EquipmentIndex.Weapon3; i += 1)
        {
            if (!equipment[i].IsEmpty && _allowedSpawnWeaponClass.Contains(equipment[i].Item.PrimaryWeapon.WeaponClass))
            {
                return true;
            }
        }

        return false;
    }

    private void ResetSpawnTeams()
    {
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (crpgPeer != null && networkPeer.ControlledAgent == null)
            {
                crpgPeer.LastSpawnInfo = null;
            }
        }
    }

    private new void OnAllAgentsFromPeerSpawnedFromVisuals(MissionPeer peer)
    {
        if (peer.ControlledFormation != null)
        {
            peer.ControlledFormation.OnFormationDispersed();
            peer.ControlledFormation.SetMovementOrder(MovementOrder.MovementOrderFollow(peer.ControlledAgent));
            NetworkCommunicator networkPeer = peer.GetNetworkPeer();
            if (peer.BotsUnderControlAlive != 0 || peer.BotsUnderControlTotal != 0)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new BotsControlledChange(networkPeer, peer.BotsUnderControlAlive, peer.BotsUnderControlTotal));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                Mission.GetMissionBehavior<MissionMultiplayerGameModeFlagDominationClient>().OnBotsControlledChanged(peer, peer.BotsUnderControlAlive, peer.BotsUnderControlTotal);
            }

            if (peer.Team == Mission.AttackerTeam)
            {
                Mission.NumOfFormationsSpawnedTeamOne++;
            }
            else
            {
                Mission.NumOfFormationsSpawnedTeamTwo++;
            }

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new SetSpawnedFormationCount(Mission.NumOfFormationsSpawnedTeamOne, Mission.NumOfFormationsSpawnedTeamTwo));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }
    }

    private int ComputeTeamSumOfEquipmentValue(Team team)
    {
        var peers = GameNetwork.NetworkPeers;
        var teamRelevantPeers =
            peers.Where(p => IsNetworkPeerRelevant(p) && p.GetComponent<MissionPeer>().Team == team).ToList();
        int valueToReturn = teamRelevantPeers.Sum(p => ComputeEquipmentValue(p.GetComponent<CrpgPeer>()));
        return Math.Max(valueToReturn, 1);
    }

    private int ComputeTeamAverageUnitValue(Team team, int teamSumOfEquipment)
    {
        var peers = GameNetwork.NetworkPeers;
        var teamRelevantPeers =
    peers.Where(p => IsNetworkPeerRelevant(p) && p.GetComponent<MissionPeer>().Team == team).ToList();
        if (teamRelevantPeers.Count < 2)
        {
            return teamRelevantPeers.Sum(p => ComputeEquipmentValue(p.GetComponent<CrpgPeer>()));
        }

        double sumOfEachSquared = teamRelevantPeers.Sum(p => Math.Pow(ComputeEquipmentValue(p.GetComponent<CrpgPeer>()), 2f));
        int valueToReturn = (int)((teamSumOfEquipment - sumOfEachSquared / teamSumOfEquipment) / (float)(teamRelevantPeers.Count - 1));
        return Math.Max(valueToReturn, 1);
    }

    private int ComputeEquipmentValue(CrpgPeer peer)
    {
        int value = peer.User?.Character.EquippedItems.Select(i => MBObjectManager.Instance.GetObject<ItemObject>(i.UserItem.ItemId)).Sum(io => io.Value) ?? 0;
#if CRPG_SERVER
        return value + CrpgServerConfiguration.BaseNakedEquipmentValue; // protection against naked
#else
        return value + 10000;
#endif
    }

    private bool IsNetworkPeerRelevant(NetworkCommunicator networkPeer)
    {
        MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
        CrpgPeer crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        bool isRelevant = !(!networkPeer.IsSynchronized
                            || missionPeer == null
                            || missionPeer.Team == null
                            || missionPeer.Team == Mission.SpectatorTeam
                            || crpgPeer == null
                            || crpgPeer.UserLoading
                            || crpgPeer.User == null);
        return isRelevant;
    }
}

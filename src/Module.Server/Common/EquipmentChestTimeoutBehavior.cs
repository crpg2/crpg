using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.ChatCommands;
using Crpg.Module.Common.Commander;
using Crpg.Module.Modes.Dtv;
using Crpg.Module.Notifications;
using Crpg.Module.Rating;
using Crpg.Module.Rewards;
using Crpg.Module.Scripts;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;
using Platform = Crpg.Module.Api.Models.Users.Platform;


namespace Crpg.Module.Common;

internal class EquipmentChestTimeoutBehavior : MissionBehavior
{

    public event Action<CrpgPeer> OnEquipChestUsed = default!;

    private Dictionary<CrpgPeer, int> timeSinceLastRearmed = new Dictionary<CrpgPeer, int>();
    private readonly CrpgRewardServer _rewardServer;
    private readonly ICrpgClient _crpgClient;
    private float RearmTimeout = 3000; // Time between equipment chest uses in ms
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    private CrpgUserManagerServer? _crpgUserManager;

    public EquipmentChestTimeoutBehavior(CrpgRewardServer rewardServer, ICrpgClient crpgClient)
    {
        _rewardServer = rewardServer;
        _crpgClient = crpgClient;
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        if (agent == null)
        {
            return;
        }

        if (agent.Equipment == null)
        {
            return;
        }

        if (!agent.IsAIControlled)
        {
            var networkPeer = agent.MissionPeer.GetNetworkPeer();

            if (networkPeer != null)
            {

                CrpgPeer? crpgPeer = networkPeer.GetComponent<CrpgPeer>();
                DateTime dateTime = DateTime.Now;

                if (timeSinceLastRearmed.ContainsKey(crpgPeer))
                {
                    timeSinceLastRearmed[crpgPeer] = (int)dateTime.TimeOfDay.TotalMilliseconds;
                }
                else
                {
                    timeSinceLastRearmed.Add(crpgPeer, (int)dateTime.TimeOfDay.TotalMilliseconds);

                }
            }
        }
    }

    public override async void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
    {
        if (usedObject.GetType() == typeof(Scripts.CrpgStandingPoint))
        {
            CrpgStandingPoint crpgPoint = (CrpgStandingPoint)usedObject;
            if (crpgPoint != null)
            {
                if (crpgPoint.getParentUsableMachine().GetType() == typeof(EquipmentChest))
                {
                    var networkPeer = userAgent.MissionPeer.GetNetworkPeer();

                    if (networkPeer != null)
                    {
                        CrpgPeer? crpgPeer = networkPeer.GetComponent<CrpgPeer>();
                        DateTime dateTime = DateTime.Now;
                        float timeDiff = (float)dateTime.TimeOfDay.TotalMilliseconds - timeSinceLastRearmed[crpgPeer];

                        if (timeDiff > RearmTimeout)
                        {
                            base.OnObjectUsed(userAgent, usedObject);
                            OnEquipChestUsed?.Invoke(crpgPeer);

                            _crpgUserManager = Mission.GetMissionBehavior<CrpgUserManagerServer>();
                            CrpgUser crpgUser = await _crpgUserManager.GetUpdatedCrpgUser(networkPeer);

                            timeSinceLastRearmed[crpgPeer] = (int)dateTime.TimeOfDay.TotalMilliseconds;
                            float previousHealth = userAgent.Health;



                            Agent? newAgent = RefreshPlayer(networkPeer, crpgUser);
                            if (newAgent != null)
                            {
                                newAgent.Health = previousHealth;
                            }
                        }
                        else
                        {
                            float timeoutDuration = RearmTimeout - timeDiff;
                            GameNetwork.BeginModuleEventAsServer(networkPeer);
                            GameNetwork.WriteMessage(new CrpgDtvEquipChestTimeoutMessage { TimeoutDuration = (float)timeoutDuration });
                            GameNetwork.EndModuleEventAsServer();
                            userAgent.StopUsingGameObject();
                        }
                    }

                    return;
                }
            }
        }
    }

    public Agent? RefreshPlayer(NetworkCommunicator networkPeer, CrpgUser updatedUser)
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());

        MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
        CrpgPeer crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        Agent controlledAgent = missionPeer.ControlledAgent;


        controlledAgent.ClearEquipment();
        controlledAgent.FadeOut(true, true);

        BasicCultureObject teamCulture = missionPeer.Team == Mission.AttackerTeam ? cultureTeam1 : cultureTeam2;
        var peerClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("crpg_captain_division_1");
        var characterSkills = CrpgCharacterBuilder.CreateCharacterSkills(updatedUser!.Character.Characteristics);
        var characterXml = peerClass.HeroCharacter;

        var characterEquipment = CrpgCharacterBuilder.CreateCharacterEquipment(updatedUser.Character.EquippedItems);

        MatrixFrame spawnFrame = controlledAgent.Frame;
        var troopOrigin = new CrpgBattleAgentOrigin(characterXml, characterSkills);
        CrpgCharacterBuilder.AssignArmorsToTroopOrigin(troopOrigin, updatedUser.Character.EquippedItems.ToList());
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
        CrpgAgentComponent agentComponent = new(agent);
        agent.AddComponent(agentComponent);

        bool hasExtraSlotEquipped = characterEquipment[EquipmentIndex.ExtraWeaponSlot].Item != null;
        if (!agent.HasMount || hasExtraSlotEquipped)
        {
            agent.WieldInitialWeapons();
        }

        return agent;
    }
}

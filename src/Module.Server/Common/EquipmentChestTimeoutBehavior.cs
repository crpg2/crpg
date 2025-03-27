using System.Linq;
using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Notifications;
using Crpg.Module.Rewards;
using Crpg.Module.Scripts;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;


namespace Crpg.Module.Common;

internal class EquipmentChestTimeoutBehavior : MissionBehavior
{
    private Dictionary<CrpgPeer, int> timeSinceLastRearmed = new Dictionary<CrpgPeer, int>();
    private readonly CrpgRewardServer _rewardServer;
    private int RearmTimeout = 3000;


    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public EquipmentChestTimeoutBehavior(CrpgRewardServer rewardServer)
    {
        _rewardServer = rewardServer;
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


    public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
    {
        if (usedObject.GetType() == typeof(Scripts.CrpgStandingPoint))
        {
            CrpgStandingPoint crpgPoint = (CrpgStandingPoint)usedObject;
            if (crpgPoint != null)
            {

                Debug.Print(crpgPoint.getParentUsableMachine().GetType().ToString());

                if (crpgPoint.getParentUsableMachine().GetType() == typeof(EquipmentChest))
                {



                    var networkPeer = userAgent.MissionPeer.GetNetworkPeer();

                    if (networkPeer != null)
                    {

                        CrpgPeer? crpgPeer = networkPeer.GetComponent<CrpgPeer>();
                        DateTime dateTime = DateTime.Now;
                        int timeDiff = (int)dateTime.TimeOfDay.TotalMilliseconds - timeSinceLastRearmed[crpgPeer];

                        if (timeDiff > RearmTimeout)
                        {

                            List<CrpgUserUpdate> userUpdates = new();
                            Guid idempotencyKey = Guid.NewGuid();

                            var request = new CrpgGameUsersUpdateRequest
                            {
                                Updates = userUpdates,
                                Key = idempotencyKey.ToString(),
                            };
                            Debug.Print("hello");
                            //_rewardServer.UpdateCrpgUsersAsync(durationRewarded: 0, updateUserStats: false);
                            var task = Task.Run(async () => await _rewardServer.UpdateCrpgUsersAsync(durationRewarded: 0, updateUserStats: false));
                            base.OnObjectUsed(userAgent, usedObject);
                            timeSinceLastRearmed[crpgPeer] = (int)dateTime.TimeOfDay.TotalMilliseconds;
                            float previousHealth = userAgent.Health;

                            Agent? newAgent = RefreshPlayer(networkPeer);
                            if (newAgent != null)
                            {
                                newAgent.Health = previousHealth;
                            }
                            else
                            {
                                Debug.Print("NULLLLLL");
                                //Do something because this shouldnt be null lol
                            }
                        }
                        else
                        {

                            Debug.Print(timeDiff.ToString());
                            int nextAvailableRearm = RearmTimeout - timeDiff;

                            GameNetwork.BeginModuleEventAsServer(networkPeer);
                            GameNetwork.WriteMessage(new CrpgNotificationId
                            {
                                Type = CrpgNotificationType.Announcement,
                                TextId = "str_notification",
                                TextVariation = "rearm_gear_timeout",
                                SoundEvent = string.Empty,
                                Variables = { ["SECONDS"] = (nextAvailableRearm / 1000).ToString() },

                            });
                            GameNetwork.EndModuleEventAsServer();

                            userAgent.StopUsingGameObject();
                        }
                    }
                    return;

                }
            }
        }
    }

    public Agent? RefreshPlayer(NetworkCommunicator networkPeer)
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
        CrpgAgentComponent agentComponent = new(agent);
        agent.AddComponent(agentComponent);

        bool hasExtraSlotEquipped = characterEquipment[EquipmentIndex.ExtraWeaponSlot].Item != null;
        if (!agent.HasMount || hasExtraSlotEquipped)
        {
            agent.WieldInitialWeapons();
        }
        Debug.Print("DONE");
        return agent;
    }


}

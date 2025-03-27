using System;
using Crpg.Module.Api;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using Crpg.Module.Modes.Dtv;
using Crpg.Module.Modes.TrainingGround;
using Crpg.Module.Notifications;
using Crpg.Module.Scripts;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static System.Net.Mime.MediaTypeNames;

namespace Crpg.Module.Scripts
{
    // Token: 0x0200035A RID: 858


    public class EquipmentChest : UsableMachine
    {
        //private readonly ICrpgClient _crpgClient;

        // Token: 0x170008B1 RID: 2225
        // (get) Token: 0x06002F1A RID: 12058 RVA: 0x000C0E87 File Offset: 0x000BF087
        private static int _pickupArrowSoundFromBarrelCache
        {
            get
            {
                if (EquipmentChest._pickupArrowSoundFromBarrel == -1)
                {
                    EquipmentChest._pickupArrowSoundFromBarrel = SoundEvent.GetEventIdFromString("event:/mission/combat/pickup_arrows");
                }
                return EquipmentChest._pickupArrowSoundFromBarrel;
            }
        }

        // Token: 0x06002F1B RID: 12059 RVA: 0x000C0EA5 File Offset: 0x000BF0A5
        protected EquipmentChest()
        {
        }

        // Token: 0x06002F1C RID: 12060 RVA: 0x000C0EB4 File Offset: 0x000BF0B4
        protected override void OnInit()
        {
            Debug.Print($"HELLO INIT");

            base.OnInit();
            foreach (CrpgStandingPoint standingPoint in StandingPoints)
            {

                standingPoint.setParentUsableMachine(this);

            }
            base.SetScriptComponentToTick(this.GetTickRequirement());
            this.MakeVisibilityCheck = false;
            this._isVisible = base.GameEntity.IsVisibleIncludeParents();
        }



        private void ServerTick(float dt)
        {

            if (!IsDeactivated)
            {

                foreach (CrpgStandingPoint standingPoint in StandingPoints)
                {

                    if (standingPoint.HasUser)
                    {
                        Agent userAgent = standingPoint.UserAgent;


                    }

                }
            }
        }

        // Token: 0x06002F1E RID: 12062 RVA: 0x000C0F94 File Offset: 0x000BF194
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject textObject = new TextObject("{=bNYm3K6bf}{KEY} Rearm", null);
            textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return textObject;
        }


        // Token: 0x06002F1F RID: 12063 RVA: 0x000C0FBE File Offset: 0x000BF1BE
        public override string GetDescriptionText(GameEntity? gameEntity = null)
        {
            return new TextObject("{=bWi4aMOB}Equipment Chest", null).ToString();
        }

        // Token: 0x06002F20 RID: 12064 RVA: 0x000C0FD0 File Offset: 0x000BF1D0
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (GameNetwork.IsClientOrReplay)
            {
                return base.GetTickRequirement();
            }
            return ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel | base.GetTickRequirement();
        }

        // Token: 0x06002F21 RID: 12065 RVA: 0x000C0FE8 File Offset: 0x000BF1E8
        protected override void OnTickParallel(float dt)
        {
            this.TickAux(true);
        }

        // Token: 0x06002F22 RID: 12066 RVA: 0x000C0FF1 File Offset: 0x000BF1F1
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            ServerTick(dt);
            if (this._needsSingleThreadTickOnce)
            {
                this._needsSingleThreadTickOnce = false;
                this.TickAux(false);
            }
        }

        // Token: 0x06002F23 RID: 12067 RVA: 0x000C1010 File Offset: 0x000BF210
        private void TickAux(bool isParallel)
        {


            if (this._isVisible && !GameNetwork.IsClientOrReplay)
            {
                foreach (CrpgStandingPoint standingPoint in base.StandingPoints)
                {
                    if (standingPoint.HasUser)
                    {

                        Agent userAgent = standingPoint.UserAgent;

                        ActionIndexValueCache currentActionValue = userAgent.GetCurrentActionValue(0);
                        ActionIndexValueCache currentActionValue2 = userAgent.GetCurrentActionValue(1);
                        if (!(currentActionValue2 == ActionIndexValueCache.act_none) || (!(currentActionValue == EquipmentChest.act_pickup_down_begin) && !(currentActionValue == EquipmentChest.act_pickup_down_begin_left_stance)))
                        {
                            if (currentActionValue2 == ActionIndexValueCache.act_none && (currentActionValue == EquipmentChest.act_pickup_down_end || currentActionValue == EquipmentChest.act_pickup_down_end_left_stance))
                            {
                                if (isParallel)
                                {
                                    this._needsSingleThreadTickOnce = true;
                                }
                                else
                                {

                                    foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                                    {
                                        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
                                        MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
                                        if (crpgPeer != null)
                                        {
                                            if (crpgPeer.User != null)
                                            {
                                                //var characterEquipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);

                                                /*
                                                BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
                                                BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());


                                                Agent controlledAgent = missionPeer.ControlledAgent;

                                                controlledAgent.ClearEquipment();
                                                controlledAgent.FadeOut(true, true);

                                                BasicCultureObject teamCulture = missionPeer.Team == userAgent.Mission.AttackerTeam ? cultureTeam1 : cultureTeam2;
                                                var peerClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("crpg_captain_division_1");
                                                var characterSkills = CrpgCharacterBuilder.CreateCharacterSkills(crpgPeer.User!.Character.Characteristics);
                                                var characterXml = peerClass.HeroCharacter;


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
                                                    agentBuildData.ClothingColor1(missionPeer.Team == userAgent.Mission.AttackerTeam
                                                        ? teamCulture.Color
                                                        : teamCulture.ClothAlternativeColor);
                                                    agentBuildData.ClothingColor2(missionPeer.Team == userAgent.Mission.AttackerTeam
                                                        ? teamCulture.Color2
                                                        : teamCulture.ClothAlternativeColor2);
                                                }

                                                Agent agent = userAgent.Mission.SpawnAgent(agentBuildData);
                                                CrpgAgentComponent agentComponent = new(agent);
                                                agent.AddComponent(agentComponent);

                                                bool hasExtraSlotEquipped = characterEquipment[EquipmentIndex.ExtraWeaponSlot].Item != null;
                                                if (!agent.HasMount || hasExtraSlotEquipped)
                                                {
                                                    agent.WieldInitialWeapons();
                                                }
                                                */

                                            }
                                        }

                                    }

                                    userAgent.StopUsingGameObject(true, Agent.StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject);
                                }
                            }
                            else if (currentActionValue2 != ActionIndexValueCache.act_none || !userAgent.SetActionChannel(0, userAgent.GetIsLeftStance() ? EquipmentChest.act_pickup_down_begin_left_stance : EquipmentChest.act_pickup_down_begin, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
                            {
                                if (isParallel)
                                {
                                    this._needsSingleThreadTickOnce = true;
                                }
                                else
                                {
                                    userAgent.StopUsingGameObject(true, Agent.StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x06002F24 RID: 12068 RVA: 0x000C1258 File Offset: 0x000BF458
        public override OrderType GetOrder(BattleSideEnum side)
        {
            return OrderType.None;
        }

        // Token: 0x040013E2 RID: 5090
        private static readonly ActionIndexCache act_pickup_down_begin = ActionIndexCache.Create("act_pickup_down_begin");

        // Token: 0x040013E3 RID: 5091
        private static readonly ActionIndexCache act_pickup_down_begin_left_stance = ActionIndexCache.Create("act_pickup_down_begin_left_stance");

        // Token: 0x040013E4 RID: 5092
        private static readonly ActionIndexCache act_pickup_down_end = ActionIndexCache.Create("act_pickup_down_end");

        // Token: 0x040013E5 RID: 5093
        private static readonly ActionIndexCache act_pickup_down_end_left_stance = ActionIndexCache.Create("act_pickup_down_end_left_stance");

        // Token: 0x040013E6 RID: 5094
        private static int _pickupArrowSoundFromBarrel = -1;

        // Token: 0x040013E7 RID: 5095
        private bool _isVisible = true;

        // Token: 0x040013E8 RID: 5096
        private bool _needsSingleThreadTickOnce;

        private Dictionary<CrpgPeer, int> timeSinceLastRearmed = new Dictionary<CrpgPeer, int>();

    }
}

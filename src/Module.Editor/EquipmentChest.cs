using System;
using Crpg.Module.Scripts;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Scripts
{
    // Token: 0x0200035A RID: 858



    internal class StandingPointWithRequiredWeaponClasses : StandingPoint
    {
        // Token: 0x06000289 RID: 649 RVA: 0x0000D6A7 File Offset: 0x0000B8A7
        protected override void OnInit()
        {
            base.OnInit();
        }

        // Token: 0x0600028A RID: 650 RVA: 0x0000D6B1 File Offset: 0x0000B8B1
        public void SetRequiredWeaponClasses(List<WeaponClass> weaponClasses)
        {
            this._requiredWeaponClasses = weaponClasses;
        }

        // Token: 0x0600028B RID: 651 RVA: 0x0000D6BC File Offset: 0x0000B8BC
        public override bool IsDisabledForAgent(Agent agent)
        {
            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
            {
                bool flag = !agent.Equipment[equipmentIndex].IsEmpty && this._requiredWeaponClasses.Contains(agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass) && (!agent.Equipment[equipmentIndex].CurrentUsageItem.IsConsumable || agent.Equipment[equipmentIndex].Amount < agent.Equipment[equipmentIndex].ModifiedMaxAmount || equipmentIndex == EquipmentIndex.ExtraWeaponSlot);
                if (flag)
                {
                    return base.IsDisabledForAgent(agent);
                }
            }
            return true;
        }

        // Token: 0x0600028C RID: 652 RVA: 0x0000D783 File Offset: 0x0000B983
        public override void AfterMissionStart()
        {
            base.LockUserFrames = true;
        }

        // Token: 0x0400011F RID: 287
        private List<WeaponClass> _requiredWeaponClasses = new List<WeaponClass>
        {
            WeaponClass.Undefined
        };
    }
}
public class EquipmentChest : UsableMachine
    {
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
            //foreach (StandingPointWithWeaponRequirement standingPoint in base.StandingPoints)
            //{
             //   Debug.Print($"HELLO STANDINGPOINT");
//
  //              standingPoint.InitRequiredWeaponClasses(WeaponClass.Arrow, WeaponClass.Bolt);
     //           
   //         }
            base.SetScriptComponentToTick(this.GetTickRequirement());
            this.MakeVisibilityCheck = false;
            this._isVisible = base.GameEntity.IsVisibleIncludeParents();
        }



        private void ServerTick(float dt)
        {
            
            if (!IsDeactivated)
            {
                foreach (StandingPointWithRequiredWeaponClasses standingPoint in StandingPoints)
                {
                    standingPoint.SetRequiredWeaponClasses(this._ammoWeaponClasses);
                    if (standingPoint.HasUser)
                    {
                        Agent userAgent = standingPoint.UserAgent;
                        Debug.Print($"HELLO BELLO");



                    }
                
                }
                foreach (StandingPoint standingPoint in StandingPoints)
                {

                    if (standingPoint.HasUser)
                    {
                        Agent userAgent = standingPoint.UserAgent;
                        Debug.Print($"HELLO BELLO2");



                    }

                }
            }
        }

        // Token: 0x06002F1E RID: 12062 RVA: 0x000C0F94 File Offset: 0x000BF194
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            Debug.Print($"HELLO2");

            TextObject textObject = new TextObject("{=bNYm3K6b}{KEY} Pick Up", null);
            textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return textObject;
        }


        // Token: 0x06002F1F RID: 12063 RVA: 0x000C0FBE File Offset: 0x000BF1BE
        public override string GetDescriptionText(GameEntity? gameEntity = null)
        {
            Debug.Print($"HELLO122");

            return new TextObject("{=bWi4aMO9}Arrow Barrel", null).ToString();
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
                foreach (StandingPoint standingPoint in base.StandingPoints)
                {
                    if (standingPoint.HasUser)
                    {

                        Debug.Print($"HELLO4");

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
                                    for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                                    {
                                        if (!userAgent.Equipment[equipmentIndex].IsEmpty && (userAgent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Arrow || userAgent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Bolt) && userAgent.Equipment[equipmentIndex].Amount < userAgent.Equipment[equipmentIndex].ModifiedMaxAmount)
                                        {
                                            userAgent.SetWeaponAmountInSlot(equipmentIndex, userAgent.Equipment[equipmentIndex].ModifiedMaxAmount, true);
                                            Mission.Current.MakeSoundOnlyOnRelatedPeer(EquipmentChest._pickupArrowSoundFromBarrelCache, userAgent.Position, userAgent.Index);
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

        private List<WeaponClass> _ammoWeaponClasses = new List<WeaponClass>
        {
            WeaponClass.Arrow,
            WeaponClass.Bolt,
            WeaponClass.Cartridge,
            WeaponClass.Javelin,
            WeaponClass.ThrowingAxe,
            WeaponClass.ThrowingKnife
        };
}


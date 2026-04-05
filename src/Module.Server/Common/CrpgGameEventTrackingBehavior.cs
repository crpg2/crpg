using Crpg.Module.Api;
using Crpg.Module.Api.Models.GameEvents;
using Crpg.Module.Modes.Warmup;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgGameEventTrackingBehavior : MissionBehavior
{
    private const int FlushIntervalMilliseconds = 5000;

    private enum TargetType
    {
        Mount,
        Character,
    }

    private enum HitType
    {
        Ranged,
        Melee,
    }

    private readonly CrpgWarmupComponent? _warmupComponent;
    private readonly ICrpgClient _crpgClient;
    private readonly List<CrpgGameEvent> _buffer;
    private DateTime _nextFlushTime;

    public CrpgGameEventTrackingBehavior(CrpgWarmupComponent? warmupComponent, ICrpgClient crpgClient)
    {
        _warmupComponent = warmupComponent;
        _crpgClient = crpgClient;
        _buffer = new();
        _nextFlushTime = DateTime.Now.AddMilliseconds(FlushIntervalMilliseconds);
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void OnAgentHit(
        Agent affectedAgent,
        Agent affectorAgent,
        in MissionWeapon affectorWeapon,
        in Blow blow,
        in AttackCollisionData attackCollisionData)
    {
        var weapon = affectorWeapon;

        if (_warmupComponent is { IsInWarmup: true })
        {
            return;
        }

        // Affector must be a player
        if (affectorAgent.MissionPeer == null)
        {
            return;
        }

        var affectorCrpgPeer = affectorAgent.MissionPeer.Peer.GetComponent<CrpgPeer>();
        if (affectorCrpgPeer?.User == null)
        {
            return;
        }

        // Affected must be either a player or a mount
        if (affectedAgent.MissionPeer == null && !affectedAgent.IsMount)
        {
            return;
        }

        // Don't process self-hits or team hits for statistics
        if (affectedAgent == affectorAgent || affectedAgent.Team == affectorAgent.Team)
        {
            return;
        }

        bool isRanged = weapon.IsAnyAmmo();
        TargetType targetType = affectedAgent.IsMount ? TargetType.Mount : TargetType.Character;

        // Check if attack was blocked (by shield or weapon)
        bool isBlocked = attackCollisionData.AttackBlockedWithShield
                         || attackCollisionData.MissileBlockedWithWeapon
                         || attackCollisionData.CollisionResult == CombatCollisionResult.Blocked
                         || attackCollisionData.CollisionResult == CombatCollisionResult.Parried
                         || attackCollisionData.CollisionResult == CombatCollisionResult.ChamberBlocked;

        if (isBlocked)
        {
            // Get the blocker's CrpgPeer (affected agent)
            var blockerCrpgPeer = affectedAgent.MissionPeer?.Peer?.GetComponent<CrpgPeer>();
            if (blockerCrpgPeer?.User == null)
            {
                return; // Can't track block event for non-player blockers
            }

            CrpgGameEvent blockEvt = new()
            {
                UserId = blockerCrpgPeer.User.Id, // Blocker's user ID, not attacker's
                Type = CrpgGameEventType.Block,
                EventData = new Dictionary<CrpgGameEventField, string>
                {
                    { CrpgGameEventField.HitType, isRanged ? nameof(HitType.Ranged) : nameof(HitType.Melee) },
                    { CrpgGameEventField.TargetType, targetType.ToString() },
                },
            };

            // Determine block type and find the blocking item
            bool isShieldBlock = attackCollisionData.AttackBlockedWithShield;
            EquipmentIndex wieldedIndex;
            if (isShieldBlock)
            {
                // Damage to shield
                blockEvt.EventData![CrpgGameEventField.Damage] = attackCollisionData.InflictedDamage.ToString();
                wieldedIndex = affectedAgent.GetOffhandWieldedItemIndex();
            }
            else
            {
                // Weapon block - try to find the wielded weapon that blocked
                wieldedIndex = affectedAgent.GetPrimaryWieldedItemIndex();
                if (wieldedIndex == EquipmentIndex.None)
                {
                    wieldedIndex = affectedAgent.GetOffhandWieldedItemIndex();
                }
            }

            if (wieldedIndex != EquipmentIndex.None)
            {
                var blockingWeapon = affectedAgent.Equipment[wieldedIndex];
                if (!blockingWeapon.IsEmpty)
                {
                    blockEvt.EventData![CrpgGameEventField.WeaponClass] =
                        blockingWeapon.CurrentUsageItem.WeaponClass.ToString();
                }
            }

            _buffer.Add(blockEvt);
            return; // Don't create a hit event for blocked attacks
        }

        CrpgGameEvent evt = new()
        {
            UserId = affectorCrpgPeer.User.Id,
            Type = CrpgGameEventType.Hit,
            EventData = new Dictionary<CrpgGameEventField, string>
            {
                { CrpgGameEventField.Damage, attackCollisionData.InflictedDamage.ToString() },
                { CrpgGameEventField.HitType, isRanged ? nameof(HitType.Ranged) : nameof(HitType.Melee) },
                { CrpgGameEventField.TargetType, targetType.ToString() },
                { CrpgGameEventField.DamageType, ((DamageTypes)attackCollisionData.DamageType).ToString() },
            },
        };

        string? bodyPart = ToString(attackCollisionData.VictimHitBodyPart);

        if (bodyPart != null)
        {
            evt.EventData![CrpgGameEventField.BodyPart] = bodyPart;
        }

        if (!weapon.IsEmpty)
        {
            SetWeaponClass(weapon.CurrentUsageItem.WeaponClass, isRanged, evt, weapon);
        }

        _buffer.Add(evt);
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent? affectorAgent, AgentState agentState,
        KillingBlow blow)
    {
        if (_warmupComponent is { IsInWarmup: true })
        {
            return;
        }

        // Only process kills
        if (agentState != AgentState.Killed || affectorAgent == null)
        {
            return;
        }

        // Affector must be a player
        if (affectorAgent.MissionPeer == null)
        {
            return;
        }

        var affectorCrpgPeer = affectorAgent.MissionPeer.Peer.GetComponent<CrpgPeer>();
        if (affectorCrpgPeer?.User == null)
        {
            return;
        }

        // Affected must be either a player or a mount
        if (affectedAgent.MissionPeer == null && !affectedAgent.IsMount)
        {
            return;
        }

        // Don't process self-kills or team kills for statistics
        if (affectedAgent == affectorAgent || affectedAgent.Team == affectorAgent.Team)
        {
            return;
        }

        TargetType targetType = affectedAgent.IsMount ? TargetType.Mount : TargetType.Character;

        CrpgGameEvent evt = new()
        {
            UserId = affectorCrpgPeer.User.Id,
            Type = CrpgGameEventType.Kill,
            EventData = new Dictionary<CrpgGameEventField, string>
            {
                { CrpgGameEventField.TargetType, targetType.ToString() },
                { CrpgGameEventField.DamageType, blow.DamageType.ToString() },
            },
        };

        // Get weapon info from currently wielded weapon
        EquipmentIndex weaponIndex = affectorAgent.GetPrimaryWieldedItemIndex();

        if (weaponIndex != EquipmentIndex.None)
        {
            var weapon = affectorAgent.Equipment[weaponIndex];
            if (!weapon.IsEmpty)
            {
                // Hit type (ranged or melee)
                bool isRanged = blow.IsMissile;
                evt.EventData![CrpgGameEventField.HitType] = isRanged ? nameof(HitType.Ranged) : nameof(HitType.Melee);

                SetWeaponClass((WeaponClass)blow.WeaponClass, isRanged, evt, weapon);
            }
        }

        string? bodyPart = ToString(blow.VictimBodyPart);
        if (bodyPart != null)
        {
            evt.EventData![CrpgGameEventField.BodyPart] = bodyPart;
        }

        _buffer.Add(evt);
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        if (DateTime.Now >= _nextFlushTime && _buffer.Count > 0)
        {
            FlushBuffer();
            _nextFlushTime = DateTime.Now.AddMilliseconds(FlushIntervalMilliseconds);
        }
    }

    private static void SetWeaponClass(WeaponClass weaponClass, bool isRanged, CrpgGameEvent evt, MissionWeapon weapon)
    {
        // Unfortunately, the engine does not allow you to get the type of ranged weapon,
        // since the damage is registered for the projectile
        if (isRanged)
        {
            switch (weaponClass)
            {
                case WeaponClass.Arrow:
                    evt.EventData![CrpgGameEventField.WeaponClass] = nameof(WeaponClass.Bow);
                    break;
                case WeaponClass.Bolt:
                    evt.EventData![CrpgGameEventField.WeaponClass] = nameof(WeaponClass.Crossbow);
                    break;
                case WeaponClass.Cartridge:
                    evt.EventData![CrpgGameEventField.WeaponClass] = nameof(WeaponClass.Musket);
                    break;
            }
        }
        else
        {
            evt.EventData![CrpgGameEventField.WeaponClass] = weapon.CurrentUsageItem.WeaponClass.ToString();
        }
    }

    private void FlushBuffer()
    {
        var events = _buffer.ToArray();
        _buffer.Clear();
        _ = _crpgClient.CreateGameEventsAsync(events); // Fire and forget
        Debug.Print($"Sent {events.Length} game events");
    }

    private string? ToString(BoneBodyPartType partType)
    {
        switch (partType)
        {
            case BoneBodyPartType.Head:
                return nameof(BoneBodyPartType.Head); // Conflict with value CriticalBodyPartsBegin
            case BoneBodyPartType.ArmLeft:
                return nameof(BoneBodyPartType.ArmLeft); // Conflict with value CriticalBodyPartsEnd
            case BoneBodyPartType.Neck:
            case BoneBodyPartType.Chest:
            case BoneBodyPartType.Abdomen:
            case BoneBodyPartType.ShoulderLeft:
            case BoneBodyPartType.ShoulderRight:
            case BoneBodyPartType.ArmRight:
            case BoneBodyPartType.Legs:
                return partType.ToString(); // Other valid cases
            default:
                return null;
        }
    }
}

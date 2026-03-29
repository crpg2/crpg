using Crpg.Module.Common.Network;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class FireWeaponsBehaviorClient : MissionNetwork
{
    public struct AgentFireWeaponData
    {
        public static readonly AgentFireWeaponData Default = new()
        {
            Enabled = false,
            ParticleSystemId = string.Empty,
            LightColor = new Vec3(1f, 0.5f, 0f),
            LightRadius = 1.5f,
            LightIntensity = 1f,
        };

        public bool Enabled { get; set; }
        public string ParticleSystemId { get; set; }
        public Vec3 LightColor { get; set; }
        public float LightRadius { get; set; }
        public float LightIntensity { get; set; }
    }

    public struct AgentFireWeaponVisuals
    {
        public Light? Light { get; set; }
        public List<ParticleSystem> Particles { get; set; }
        public WeakGameEntity? WeaponEntity { get; set; }
    }

    public struct ArrowFireVisuals
    {
        public Mission.Missile Mmissile { get; set; }
        public MissionTimer MTimer { get; set; }
        public ParticleSystem MParticle { get; set; }
        public bool MDelete { get; set; }
    }

    private static readonly HashSet<ItemObject.ItemTypeEnum> RangedItemTypes =
    [
        ItemObject.ItemTypeEnum.Bow,
        ItemObject.ItemTypeEnum.Arrows,
        ItemObject.ItemTypeEnum.Crossbow,
        ItemObject.ItemTypeEnum.Bolts,
        ItemObject.ItemTypeEnum.Thrown,
        ItemObject.ItemTypeEnum.Musket,
        ItemObject.ItemTypeEnum.Bullets,
];

    private readonly Dictionary<Agent, AgentFireWeaponData> _agentFireWeaponData = [];
    private readonly Dictionary<Agent, AgentFireWeaponVisuals> _agentFireWeaponVisuals = [];
    private readonly List<ArrowFireVisuals> _hitArrowFires = [];
    private Dictionary<Mission.Missile, ParticleSystem> _missileParticles = [];

    public override void EarlyStart()
    {
        base.EarlyStart();
        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new UserRequestUpdateFireWeapons());
        GameNetwork.EndModuleEventAsClient();
    }

    public override void OnAgentDeleted(Agent affectedAgent)
    {
        base.OnAgentDeleted(affectedAgent);
        RemoveFireWeaponVisuals(affectedAgent);
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
        RemoveFireWeaponVisuals(affectedAgent);
    }

    public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
    {
        base.OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);

        if (!_agentFireWeaponData.TryGetValue(shooterAgent, out AgentFireWeaponData data) || !data.Enabled)
        {
            return;
        }

        MissionWeapon weapon = shooterAgent.Equipment[weaponIndex];
        if (RangedItemTypes.Contains(weapon.Item?.ItemType ?? ItemObject.ItemTypeEnum.Invalid))
        {
            foreach (Mission.Missile missile in Mission.Current.MissilesList)
            {
                if (!_missileParticles.ContainsKey(missile))
                {
                    MatrixFrame localFrame = new(Mat3.Identity, new Vec3(0, 0, 0));
                    ParticleSystem particle = ParticleSystem.CreateParticleSystemAttachedToEntity(
                        data.ParticleSystemId, missile.Entity, ref localFrame);
                    Light light = Light.CreatePointLight(data.LightRadius);
                    light.Intensity = data.LightIntensity;
                    light.LightColor = data.LightColor;

                    missile.Entity.AddLight(light);
                    _missileParticles.Add(missile, particle);
                }
            }
        }
    }

    public override void OnMissileCollisionReaction(Mission.MissileCollisionReaction collisionReaction, Agent attackerAgent, Agent attachedAgent, sbyte attachedBoneIndex)
    {
        base.OnMissileCollisionReaction(collisionReaction, attackerAgent, attachedAgent, attachedBoneIndex);
        var existing = new Dictionary<Mission.Missile, ParticleSystem>();
        foreach (Mission.Missile missile in Mission.Current.MissilesList)
        {
            if (_missileParticles.ContainsKey(missile))
            {
                existing.Add(missile, _missileParticles[missile]);
                _missileParticles.Remove(missile);
            }
        }

        foreach (KeyValuePair<Mission.Missile, ParticleSystem> item in _missileParticles)
        {
            Mission.Missile missile = item.Key;
            Light light = missile.Entity.GetLight();

            if (attachedAgent != null)
            {
                missile.Entity.RemoveAllParticleSystems();
                if (light != null)
                {
                    missile.Entity.RemoveComponent(light);
                }
            }
            else
            {
                if (!_agentFireWeaponData.TryGetValue(attackerAgent, out AgentFireWeaponData data) || !data.Enabled)
                {
                    continue;
                }

                ParticleSystem particle = item.Value;
                MatrixFrame localFrame = particle.GetLocalFrame().Elevate(0.6f);
                particle.SetLocalFrame(in localFrame);
                if (light != null)
                {
                    light.Frame = light.Frame.Elevate(0.15f);
                    light.Intensity = data.LightIntensity;
                }

                localFrame = new MatrixFrame(Mat3.Identity, new Vec3(0, 0, 0)).Elevate(0.6f);

                var arrowData = new ArrowFireVisuals
                {
                    Mmissile = missile,
                    MTimer = new MissionTimer(5f),
                    MParticle = ParticleSystem.CreateParticleSystemAttachedToEntity(data.ParticleSystemId, missile.Entity, ref localFrame),
                };
                _hitArrowFires.Add(arrowData);
            }
        }

        _missileParticles = existing;
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        if (_hitArrowFires.Count > 0)
        {
            List<ArrowFireVisuals> deleteItems = [];
            foreach (ArrowFireVisuals item in _hitArrowFires)
            {
                if (item.MTimer.Check(false))
                {
                    GameEntity entity = item.Mmissile.Entity;
                    if (entity != null)
                    {
                        entity.RemoveAllParticleSystems();
                        Light light = entity.GetLight();
                        if (light != null)
                        {
                            entity.RemoveComponent(light);
                        }
                    }

                    deleteItems.Add(item);
                }
            }

            foreach (ArrowFireVisuals item in deleteItems)
            {
                _hitArrowFires.Remove(item);
            }
        }
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<UpdateFireWeapon>(HandleUpdateFireWeapon);
        registerer.Register<UpdateAllFireWeapons>(HandleUpdateAllFireWeapons);
        registerer.Register<SetWeaponReloadPhase>(HandleSetWeaponReloadPhase);
        registerer.Register<SetWieldedItemIndex>(HandleSetWieldedItemIndex);
    }

    private void HandleSetWieldedItemIndex(SetWieldedItemIndex message)
    {
        Agent agent = Mission.MissionNetworkHelper.GetAgentFromIndex(message.AgentIndex, true);
        if (agent == null || agent.IsActive())
        {
            return;
        }

        EquipmentIndex slot = agent.GetPrimaryWieldedItemIndex();
        if (slot == EquipmentIndex.None)
        {
            RemoveFireWeaponVisuals(agent);
            return;
        }

        MissionWeapon weapon = agent.Equipment[slot];
        if (weapon.IsEqualTo(MissionWeapon.Invalid) || weapon.IsEmpty || weapon.Item == null || !IsValidFireWeapon(weapon))
        {
            RemoveFireWeaponVisuals(agent);
            return;
        }

        if (_agentFireWeaponData.TryGetValue(agent, out AgentFireWeaponData data) && data.Enabled == true)
        {
            ApplyFireWeaponEffects(agent, _agentFireWeaponData[agent]);
        }
    }

    private void HandleUpdateFireWeapon(UpdateFireWeapon message)
    {
        Agent agent = Mission.MissionNetworkHelper.GetAgentFromIndex(message.AgentIndex, true);
        if (agent == null)
        {
            return;
        }

        var data = new AgentFireWeaponData
        {
            Enabled = message.Enabled,
            ParticleSystemId = message.ParticleSystemId,
            LightColor = message.LightColor,
            LightRadius = message.LightRadius,
            LightIntensity = message.LightIntensity,
        };

        _agentFireWeaponData[agent] = data;

        if (data.Enabled)
        {
            ApplyFireWeaponEffects(agent, _agentFireWeaponData[agent]);
        }
        else
        {
            RemoveFireWeaponVisuals(agent);
        }
    }

    private void HandleUpdateAllFireWeapons(UpdateAllFireWeapons message)
    {
        foreach (var entry in message.Entries)
        {
            Agent agent = Mission.MissionNetworkHelper.GetAgentFromIndex(entry.AgentIndex, true);
            if (agent == null)
            {
                continue;
            }

            var data = new AgentFireWeaponData
            {
                Enabled = entry.Enabled,
                ParticleSystemId = entry.ParticleSystemId,
                LightColor = entry.LightColor,
                LightRadius = entry.LightRadius,
                LightIntensity = entry.LightIntensity,
            };

            _agentFireWeaponData[agent] = data;

            if (data.Enabled)
            {
                ApplyFireWeaponEffects(agent, data);
            }
            else
            {
                RemoveFireWeaponVisuals(agent);
            }
        }
    }

    private void HandleSetWeaponReloadPhase(SetWeaponReloadPhase message)
    {
        Agent agent = Mission.MissionNetworkHelper.GetAgentFromIndex(message.AgentIndex, true);
        EquipmentIndex equipmentIndex = message.EquipmentIndex;
        short reloadPhase = message.ReloadPhase;

        MissionWeapon weapon = agent.Equipment[equipmentIndex];

        if (agent == Agent.Main)
        {
            InformationManager.DisplayMessage(new InformationMessage($"HandleSetWeaponReloadPhase({reloadPhase}) count: {weapon.ReloadPhaseCount}"));
        }

        if (reloadPhase > 0)
        {
            if (_agentFireWeaponData.TryGetValue(agent, out AgentFireWeaponData data) && data.Enabled == true)
            {
                ApplyFireWeaponEffects(agent, _agentFireWeaponData[agent]);
            }
        }
        else
        {
            RemoveFireWeaponVisuals(agent);
        }
    }

    private void ApplyFireWeaponEffects(Agent agent, AgentFireWeaponData data)
    {
        if (agent == null || !agent.IsActive())
        {
            return;
        }

        // Clean up existing visuals before applying new ones
        RemoveFireWeaponVisuals(agent);

        if (!data.Enabled)
        {
            return;
        }

        EquipmentIndex equipmentIndex = agent.GetPrimaryWieldedItemIndex();
        if (equipmentIndex == EquipmentIndex.None)
        {
            return;
        }

        WeakGameEntity weaponEntity = agent.GetWeaponEntityFromEquipmentSlot(equipmentIndex);
        MissionWeapon missionWeapon = agent.WieldedWeapon;
        if (missionWeapon.IsEmpty || missionWeapon.Item == null) // || !IsValidFireWeapon(missionWeapon))
        {
            return;
        }

        MBAgentVisuals agentVisuals = agent.AgentVisuals;
        if (agentVisuals == null)
        {
            return;
        }

        int wLength = missionWeapon.GetWeaponStatsData()[0].WeaponLength;
        int count = (int)Math.Round(wLength / 10f);

        Skeleton skeleton = agentVisuals.GetSkeleton();
        var particles = new List<ParticleSystem>();
        Light light;

        if (missionWeapon.Item?.ItemType == ItemObject.ItemTypeEnum.Bow && missionWeapon.Ammo > 0)
        {
            // ranged
            WeaponData wData = missionWeapon.GetAmmoWeaponData(false);
            MissionWeapon aWeapon = missionWeapon.AmmoWeapon;
            MatrixFrame aFrame = wData.WeaponFrame;
            Mat3 weaponRotation = aFrame.rotation;

            wLength = aWeapon.GetWeaponStatsData()[0].WeaponLength;
            count = 2;
            weaponEntity = agent.GetWeaponEntityFromEquipmentSlot(GetEquipmentIndexFromWeapon(agent, aWeapon));
            weaponEntity.GetLocalFrame(out aFrame);

            for (int i = 1; i < count; i++)
            {
                float angleRad = 20f * (float)(Math.PI / 180f);
                float cos = (float)Math.Cos(angleRad);
                float sin = (float)Math.Sin(angleRad);

                // Rotate the offset vector around Z-axis
                Vec3 baseOffset = new(-wLength / 100f * 0.9f, 0, 0);
                Vec3 rotatedOffset = new(
                    baseOffset.x * cos - baseOffset.y * sin,
                    baseOffset.x * sin + baseOffset.y * cos,
                    baseOffset.z + 0.2f);

                MatrixFrame particleFrame = new(aFrame.rotation, rotatedOffset);
                ParticleSystem particle = ParticleSystem.CreateParticleSystemAttachedToEntity(data.ParticleSystemId,
                    weaponEntity, ref particleFrame);

                if (particle == null)
                {
                    continue;
                }

                skeleton.AddComponentToBone(Game.Current.DefaultMonster.MainHandItemBoneIndex, particle);
                particles.Add(particle);
            }

            light = Light.CreatePointLight(data.LightRadius);
            light.Intensity = data.LightIntensity;
            // Move light to tip along Y axis
            light.Frame = new MatrixFrame(Mat3.Identity, new Vec3(0, (count - 1) * 0.1f, 0));
            light.SetVisibility(true);
        }

        /*
        else if (missionWeapon.Item?.ItemType == ItemObject.ItemTypeEnum.Crossbow)
        {

        }*/
        else
        {
            for (int i = 1; i < count; i++)
            {
                MatrixFrame localFrame = new MatrixFrame(Mat3.Identity, new Vec3(0, 0, 0)).Elevate(i * 0.1f);
                ParticleSystem particle = ParticleSystem.CreateParticleSystemAttachedToEntity(data.ParticleSystemId,
                    weaponEntity, ref localFrame);
                if (particle == null)
                {
                    continue;
                }

                skeleton.AddComponentToBone(Game.Current.DefaultMonster.MainHandItemBoneIndex, particle);
                particles.Add(particle);
            }

            light = Light.CreatePointLight(data.LightRadius);
            light.Intensity = data.LightIntensity;
            light.Frame = light.Frame.Elevate((count - 1) * 0.1f);
            light.SetVisibility(true);
        }

        skeleton.AddComponentToBone(Game.Current.DefaultMonster.MainHandItemBoneIndex, light);

        _agentFireWeaponVisuals[agent] = new AgentFireWeaponVisuals
        {
            Light = light,
            Particles = particles,
            WeaponEntity = weaponEntity,
        };

        agent.AgentVisuals.LazyUpdateAgentRendererData();
    }

    private static EquipmentIndex GetEquipmentIndexFromWeapon(Agent agent, MissionWeapon weapon)
    {
        for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
        {
            if (agent.Equipment[i].Item == weapon.Item)
            {
                return i;
            }
        }

        return EquipmentIndex.None;
    }

    private void RemoveFireWeaponVisuals(Agent agent)
    {
        if (!_agentFireWeaponVisuals.TryGetValue(agent, out var visuals))
        {
            return;
        }

        if (visuals.WeaponEntity != null && agent != null)
        {
            MBAgentVisuals agentVisuals = agent.AgentVisuals;
            if (agentVisuals != null)
            {
                Skeleton skeleton = agentVisuals.GetSkeleton();
                if (visuals.Light != null && skeleton != null)
                {
                    skeleton.RemoveComponent(visuals.Light);
                    foreach (var p in visuals.Particles)
                    {
                        skeleton.RemoveComponent(p);
                    }
                }
            }

            _agentFireWeaponVisuals.Remove(agent);
        }
    }

    private static readonly HashSet<WeaponClass> ValidFireWeaponClasses =
    [
        WeaponClass.OneHandedSword,
        WeaponClass.TwoHandedSword,
        WeaponClass.OneHandedAxe,
        WeaponClass.TwoHandedAxe,
        WeaponClass.Mace,
        WeaponClass.TwoHandedMace,
        WeaponClass.OneHandedPolearm,
        WeaponClass.TwoHandedPolearm,
        WeaponClass.LowGripPolearm,
        WeaponClass.Dagger,
        WeaponClass.Pick,
    ];

    private bool IsValidFireWeapon(MissionWeapon weapon)
    {
        return !weapon.IsEmpty
            && weapon.CurrentUsageItem != null
            && ValidFireWeaponClasses.Contains(weapon.CurrentUsageItem.WeaponClass);
    }
}

using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
/*
namespace Crpg.Module.Common.Inventory
{
    public class CrpgInventorySyncComponent : MissionNetwork
    {
        private readonly Dictionary<AgentIndex, Equipment> _spawnEquipmentCache = new();

        public static CrpgSpawnEquipmentSyncComponent Instance { get; private set; }

        protected override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            Instance = this;

            if (GameNetwork.IsClient)
            {
                GameNetwork.NetworkMessageHandlerRegister<SpawnEquipmentSyncMessage>(OnReceiveSpawnEquipmentMessage);
            }
        }

        protected override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            if (GameNetwork.IsClient)
            {
                GameNetwork.NetworkMessageHandlerUnregister<SpawnEquipmentSyncMessage>();
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public Equipment? GetSpawnEquipmentForAgent(Agent agent)
        {
            if (_spawnEquipmentCache.TryGetValue(agent.Index, out var eq))
            {
                return eq;
            }

            return null;
        }

        public void SendSpawnEquipmentToClient(Agent agent)
        {
            if (!GameNetwork.IsServer || agent.MissionPeer == null)
                return;

            Equipment equipment = agent.SpawnEquipment.Clone();
            GameNetwork.BeginModuleEventAsServer(agent.MissionPeer.Connection);
            GameNetwork.WriteMessage(new SpawnEquipmentSyncMessage(agent.Index, equipment));
            GameNetwork.EndModuleEventAsServer();
        }

        private void OnReceiveSpawnEquipmentMessage(SpawnEquipmentSyncMessage msg)
        {
            _spawnEquipmentCache[msg.AgentIndex] = msg.Equipment;
        }
    }
}
*/
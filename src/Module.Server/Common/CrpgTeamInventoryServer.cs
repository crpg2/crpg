using Crpg.Module.Api.Models.Items;
using Crpg.Module.Common.Network.TeamInventory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common;

internal class CrpgTeamInventoryServer : MissionNetwork
{
    public bool IsEnabled { get; private set; }
    private readonly Dictionary<NetworkCommunicator, bool> _pendingTeamItemsSend = [];
    public Dictionary<BattleSideEnum, List<CrpgTeamInventoryItem>> TeamItems { get; private set; } = [];
    public Dictionary<NetworkCommunicator, Dictionary<CrpgItemSlot, string>> PendingEquipment { get; private set; } = [];
    public Dictionary<NetworkCommunicator, Dictionary<CrpgItemSlot, string>> LastUsedEquipment { get; private set; } = [];
    public HashSet<NetworkCommunicator> ReadyToSpawn { get; private set; } = [];

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        SetEnabled(CrpgServerConfiguration.IsTeamInventoryEnabled);
        MissionPeer.OnTeamChanged += HandlePeerTeamChanged;
        CrpgServerConfiguration.OnCrpgServerConfigurationChanged += HandleCrpgServerConfigurationChanged;
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        MissionPeer.OnTeamChanged -= HandlePeerTeamChanged;
        CrpgServerConfiguration.OnCrpgServerConfigurationChanged -= HandleCrpgServerConfigurationChanged;
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

        if (!IsEnabled || !affectedAgent.IsPlayerControlled)
        {
            return;
        }

        NetworkCommunicator? peer = affectedAgent.MissionPeer?.GetNetworkPeer();
        if (peer == null || !peer.IsConnectionActive)
        {
            return;
        }

        TryReEquipLastUsedEquipment(peer);
        SendMessageToPeer(peer, new ServerSendForceEquipMenu
        {
            Show = true,
            ShowReadyButton = true,
            Msg = new TextObject("{=KC9dx230}Select your equipment and click ready").ToString(),
        });
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        base.OnAgentBuild(agent, banner);

        if (!IsEnabled || !agent.IsPlayerControlled)
        {
            return;
        }

        NetworkCommunicator? peer = agent.MissionPeer?.GetNetworkPeer();
        if (peer == null)
        {
            return;
        }

        // save current pending as last known loadout before clearing
        if (PendingEquipment.TryGetValue(peer, out var pending))
        {
            LastUsedEquipment[peer] = new Dictionary<CrpgItemSlot, string>(pending);
            SendMessageToPeer(peer, new ServerSendLastUsedEquipment { Equipment = LastUsedEquipment[peer] });

            BattleSideEnum team = peer.GetComponent<MissionPeer>()?.Team?.Side ?? BattleSideEnum.None;
            if (team == BattleSideEnum.None)
            {
                return;
            }

            var remaining = pending
               .GroupBy(kvp => kvp.Value)
               .ToDictionary(g => g.Key, g => FindItem(team, g.Key)?.Quantity ?? 0);

            BroadcastToTeam(team, new ServerSendTeamItemsQuantityUpdated
            {
                Peer = peer.VirtualPlayer,
                Items = remaining,
            });
        }

        ReadyToSpawn.Remove(peer); // clear ready state
        PendingEquipment.Remove(peer);
    }

    public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
    {
        ReadyToSpawn.Remove(networkPeer);
        LastUsedEquipment.Remove(networkPeer);
        ReturnPendingEquipmentForPeer(networkPeer);
        _pendingTeamItemsSend.Remove(networkPeer);
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        if (_pendingTeamItemsSend.Count == 0)
        {
            return;
        }

        foreach (var peer in _pendingTeamItemsSend.Keys.ToList())
        {
            _pendingTeamItemsSend.Remove(peer);

            if (!peer.IsConnectionActive)
            {
                continue;
            }

            SendPeerTeamItemsList(peer);
            SendMessageToPeer(peer, new ServerSendForceEquipMenu
            {
                Show = true,
                ShowReadyButton = true,
                Msg = new TextObject("{=KC9dx230}Select your equipment and click ready").ToString(),
            });
        }
    }

    internal Equipment GetPendingEquipment(NetworkCommunicator peer)
    {
        if (!PendingEquipment.TryGetValue(peer, out var pending) || pending.Count == 0)
        {
            return new Equipment();
        }

        var equippedItems = pending
            .Select(kvp => new CrpgEquippedItem
            {
                Slot = kvp.Key,
                UserItem = new CrpgUserItem { Id = CrpgTeamInventoryClient.TeamInventoryItemId, ItemId = kvp.Value },
            })
            .ToList();

        return CrpgCharacterBuilder.CreateCharacterEquipment(equippedItems);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        registerer.Register<UserRequestEquipTeamItem>(HandleUserRequestEquipTeamItem);
        registerer.Register<UserRequestGetTeamItemsList>(HandleUserRequestGetTeamItemsList);
        registerer.Register<UserRequestSpawnWithTeamEquipment>(HandleUserRequestSpawnWithTeamEquipment);
        registerer.Register<UserRequestReEquipTeamItems>(HandleUserRequestReEquipTeamItems);
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator peer)
    {
        base.HandleEarlyNewClientAfterLoadingFinished(peer);
        SendMessageToPeer(peer, new ServerSendTeamInventoryEnabled { IsEnabled = CrpgServerConfiguration.IsTeamInventoryEnabled });
    }

    private void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;

        // Todo make it where can load team inventory from API
        // GenerateFakeInventory();
        var attackerItems = GenerateRandomTeamItems();
        var defenderItems = GenerateRandomTeamItems();

        TeamItems[BattleSideEnum.Attacker] = attackerItems;
        TeamItems[BattleSideEnum.Defender] = defenderItems;

        if (!GameNetwork.NetworkPeers.Any())
        {
            return;
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new ServerSendTeamInventoryEnabled { IsEnabled = enabled });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private void HandleCrpgServerConfigurationChanged(string key, object value)
    {
        switch (key)
        {
            case nameof(CrpgServerConfiguration.IsTeamInventoryEnabled):
                SetEnabled((bool)value);
                foreach (var peer in GameNetwork.NetworkPeers)
                {
                    if (peer.IsConnectionActive && peer.IsSynchronized)
                    {
                        SendMessageToPeer(peer, new ServerSendTeamInventoryEnabled { IsEnabled = CrpgServerConfiguration.IsTeamInventoryEnabled });
                        if (IsEnabled)
                        {
                            SendPeerTeamItemsList(peer);
                        }
                    }
                }

                break;
        }
    }

    private void HandlePeerTeamChanged(NetworkCommunicator peer, Team previousTeam, Team newTeam)
    {
        if (peer == null || !IsEnabled)
        {
            return;
        }

        if (newTeam == null || newTeam == Mission.Current.SpectatorTeam)
        {
            return;
        }

        ReadyToSpawn.Remove(peer);
        LastUsedEquipment.Remove(peer);
        ReturnPendingEquipmentForPeer(peer, previousTeam?.Side);

        // Mark as pending — will be sent next tick, cancelling any previous pending send (DTV workaround, rapid teamchange was causing crash)
        _pendingTeamItemsSend[peer] = true;
    }

    private void GenerateFakeInventory()
    {
        TeamItems[BattleSideEnum.Attacker] = new List<CrpgTeamInventoryItem>
        {
            new() { Id = "crpg_french_voulge_v3_h2", Quantity = 10, Restricted = false },
            new() { Id = "crpg_black_katana_v4_h3", Quantity = 5, Restricted = false },
            new() { Id = "crpg_aserai_armor_02_b_v2_h0", Quantity = 1, Restricted = false },
            new() { Id = "crpg_aserai_battle_crown_v2_h2", Quantity = 21, Restricted = false },
            new() { Id = "crpg_strong_tribal_long_bow_v2_h3", Quantity = 67, Restricted = false },
        };
        TeamItems[BattleSideEnum.Defender] = new List<CrpgTeamInventoryItem>
        {
            new() { Id = "crpg_french_voulge_v3_h2", Quantity = 10, Restricted = false },
            new() { Id = "crpg_black_katana_v4_h3", Quantity = 5, Restricted = false },
            new() { Id = "crpg_aserai_armor_02_b_v2_h0", Quantity = 1, Restricted = false },
            new() { Id = "crpg_battania_leather_boots_v2_h3", Quantity = 2, Restricted = true },
            new() { Id = "crpg_noble_short_bow_v2_h2", Quantity = 67, Restricted = false },
        };
    }

    private void AddRandomItems(List<CrpgTeamInventoryItem> items, List<ItemObject> pool, int count, int minQty, int maxQty, Random random)
    {
        if (pool.Count == 0)
        {
            return;
        }

        var picked = pool.OrderBy(_ => random.Next()).Take(count);
        foreach (var itemObj in picked)
        {
            items.Add(new CrpgTeamInventoryItem
            {
                Id = itemObj.StringId,
                Quantity = random.Next(minQty, maxQty + 1),
                Restricted = false,
            });
        }
    }

    private List<CrpgTeamInventoryItem> GenerateRandomTeamItems()
    {
        var random = new Random();
        var items = new List<CrpgTeamInventoryItem>();

        var allItems = MBObjectManager.Instance.GetObjectTypeList<ItemObject>()
            .Where(i => i.StringId.StartsWith("crpg_"))
            .ToList();

        // filter pools by item type
        var oneHandedWeapons = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon).ToList();
        var twoHandedWeapons = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon).ToList();
        var polearms = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.Polearm).ToList();
        var bows = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.Bow).ToList();
        var arrows = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.Arrows).ToList();
        var crossbows = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.Crossbow).ToList();
        var thrown = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.Thrown).ToList();
        var shields = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.Shield).ToList();

        // split armor by tier/weight using armor values
        var allHelms = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.HeadArmor).ToList();
        var allBodies = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.BodyArmor).ToList();
        var allGloves = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.HandArmor).ToList();
        var allBoots = allItems.Where(i => i.ItemType == ItemObject.ItemTypeEnum.LegArmor).ToList();

        // split armor into light/medium/heavy by armor value
        var lightHelms = allHelms.Where(i => i.ArmorComponent.HeadArmor < 20).ToList();
        var mediumHelms = allHelms.Where(i => i.ArmorComponent.HeadArmor >= 20 && i.ArmorComponent.HeadArmor < 40).ToList();
        var heavyHelms = allHelms.Where(i => i.ArmorComponent.HeadArmor >= 40).ToList();

        var lightBodies = allBodies.Where(i => i.ArmorComponent.BodyArmor < 30).ToList();
        var mediumBodies = allBodies.Where(i => i.ArmorComponent.BodyArmor >= 30 && i.ArmorComponent.BodyArmor < 60).ToList();
        var heavyBodies = allBodies.Where(i => i.ArmorComponent.BodyArmor >= 60).ToList();

        var lightGloves = allGloves.Where(i => i.ArmorComponent.ArmArmor < 15).ToList();
        var mediumGloves = allGloves.Where(i => i.ArmorComponent.ArmArmor >= 15).ToList();

        var lightBoots = allBoots.Where(i => i.ArmorComponent.LegArmor < 15).ToList();
        var mediumBoots = allBoots.Where(i => i.ArmorComponent.LegArmor >= 15).ToList();

        // pick weapons
        AddRandomItems(items, oneHandedWeapons, count: 5, minQty: 5, maxQty: 15, random);
        AddRandomItems(items, twoHandedWeapons, count: 5, minQty: 3, maxQty: 10, random);
        AddRandomItems(items, polearms, count: 3, minQty: 3, maxQty: 10, random);
        AddRandomItems(items, bows, count: 2, minQty: 5, maxQty: 20, random);
        AddRandomItems(items, shields, count: 2, minQty: 5, maxQty: 15, random);
        AddRandomItems(items, arrows, count: 3, minQty: 40, maxQty: 300, random);

        // light armor set
        AddRandomItems(items, lightHelms, count: 2, minQty: 3, maxQty: 8, random);
        AddRandomItems(items, lightBodies, count: 2, minQty: 3, maxQty: 8, random);
        AddRandomItems(items, lightGloves, count: 2, minQty: 3, maxQty: 8, random);
        AddRandomItems(items, lightBoots, count: 2, minQty: 3, maxQty: 8, random);

        // medium armor set
        AddRandomItems(items, mediumHelms, count: 3, minQty: 2, maxQty: 5, random);
        AddRandomItems(items, mediumBodies, count: 3, minQty: 2, maxQty: 5, random);
        AddRandomItems(items, mediumGloves, count: 2, minQty: 2, maxQty: 5, random);
        AddRandomItems(items, mediumBoots, count: 2, minQty: 2, maxQty: 5, random);

        // heavy armor set
        AddRandomItems(items, heavyHelms, count: 3, minQty: 1, maxQty: 3, random);
        AddRandomItems(items, heavyBodies, count: 3, minQty: 1, maxQty: 3, random);

        return items;
    }

    private CrpgTeamInventoryItem? FindItem(List<CrpgTeamInventoryItem> items, string id)
    {
        return items.FirstOrDefault(i => i.Id == id);
    }

    private CrpgTeamInventoryItem? FindItem(BattleSideEnum team, string id)
    {
        if (!TeamItems.TryGetValue(team, out var items))
        {
            return null;
        }

        return items.FirstOrDefault(i => i.Id == id);
    }

    private bool TryEquipItem(BattleSideEnum team, string id, bool equip, CrpgItemSlot slot)
    {
        Debug.Print($"TryEquipItem({team.ToString()}, {id}, {equip}, {slot})", 0, Debug.DebugColor.Yellow);
        CrpgTeamInventoryItem? item = FindItem(team, id);

        if (item == null)
        {
            return false;
        }

        // todo: add some check for clan/rank to bypass restriction
        if (item.Restricted)
        {
            return false;
        }

        if (equip)
        {
            if (item.Quantity <= 0)
            {
                return false;
            }

            item.Quantity--;
        }
        else
        {
            item.Quantity++;
        }

        return true;
    }

    private void TryReEquipLastUsedEquipment(NetworkCommunicator peer)
    {
        if (!LastUsedEquipment.TryGetValue(peer, out var lastLoadout) || lastLoadout.Count == 0)
        {
            return;
        }

        BattleSideEnum team = peer.GetComponent<MissionPeer>()?.Team?.Side ?? BattleSideEnum.None;
        if (!TeamItems.ContainsKey(team))
        {
            return;
        }

        foreach (var kvp in lastLoadout)
        {
            CrpgItemSlot slot = kvp.Key;
            string itemId = kvp.Value;

            if (!TryEquipItem(team, itemId, true, slot))
            {
                SendPeerEquipResult(peer, itemId, false, slot, 0);
                continue;
            }

            var teamItem = FindItem(team, itemId);
            if (teamItem == null)
            {
                SendPeerEquipResult(peer, itemId, false, slot, 0);
                continue;
            }

            SetItemPendingForPeer(peer, itemId, slot);
            BroadcastToTeam(team, new ServerSendTeamItemQuantityUpdated { Peer = peer.VirtualPlayer, Item = itemId, Quantity = teamItem.Quantity });
            SendPeerEquipResult(peer, itemId, true, slot, teamItem.Quantity);
        }
    }

    private void SetItemPendingForPeer(NetworkCommunicator peer, string itemId, CrpgItemSlot slot)
    {
        if (!PendingEquipment.TryGetValue(peer, out var pending))
        {
            pending = [];
            PendingEquipment[peer] = pending;
        }

        pending[slot] = itemId;
    }

    private void SetItemReturnedByPeer(NetworkCommunicator peer, CrpgItemSlot slot)
    {
        if (!PendingEquipment.TryGetValue(peer, out var pending))
        {
            return;
        }

        pending.Remove(slot);
        if (pending.Count == 0)
        {
            PendingEquipment.Remove(peer);
        }
    }

    private void ReturnPendingEquipmentForPeer(NetworkCommunicator peer, BattleSideEnum? team = null)
    {
        if (!PendingEquipment.TryGetValue(peer, out var pending) || pending.Count == 0)
        {
            return;
        }

        BattleSideEnum resolvedTeam = team ?? peer.GetComponent<MissionPeer>()?.Team?.Side ?? BattleSideEnum.None;

        if (resolvedTeam == BattleSideEnum.None)
        {
            PendingEquipment.Remove(peer);
            return;
        }

        var itemsToReturn = pending
            .GroupBy(kvp => kvp.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        if (TeamItems.TryGetValue(resolvedTeam, out var teamItems))
        {
            foreach (var kvp in itemsToReturn)
            {
                var item = FindItem(teamItems, kvp.Key);
                if (item != null)
                {
                    item.Quantity += kvp.Value;
                }
            }
        }

        BroadcastToTeam(resolvedTeam, new ServerSendTeamItemsQuantityUpdated { Peer = peer.VirtualPlayer, Items = itemsToReturn });
        PendingEquipment.Remove(peer);
    }

    private void SendPeerTeamItemsList(NetworkCommunicator peer, BattleSideEnum? team = null)
    {
        if (!peer.IsConnectionActive)
        {
            return;
        }

        BattleSideEnum resolvedTeam = team ?? peer.GetComponent<MissionPeer>()?.Team?.Side ?? BattleSideEnum.None;

        if (!TeamItems.TryGetValue(resolvedTeam, out List<CrpgTeamInventoryItem>? items))
        {
            // items = [];
            return;
        }

        SendMessageToPeer(peer, new ServerSendTeamItemsList { Items = items });
    }

    private void SendPeerEquipResult(NetworkCommunicator peer, string id, bool equipped, CrpgItemSlot slot, int quantity)
    {
        if (!peer.IsConnectionActive)
        {
            return;
        }

        SendMessageToPeer(peer, new ServerSendEquipTeamItemResult
        {
            Peer = peer.VirtualPlayer,
            Item = id,
            Equipped = equipped,
            Slot = slot,
            Quantity = quantity,
        });
    }

    private void SendMessageToPeer(NetworkCommunicator peer, GameNetworkMessage message)
    {
        if (!peer.IsConnectionActive)
        {
            return;
        }

        GameNetwork.BeginModuleEventAsServer(peer);
        GameNetwork.WriteMessage(message);
        GameNetwork.EndModuleEventAsServer();
    }

    private void BroadcastToTeam(BattleSideEnum team, GameNetworkMessage message)
    {
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            if (networkPeer.GetComponent<MissionPeer>()?.Team?.Side == team)
            {
                SendMessageToPeer(networkPeer, message);
            }
        }
    }

    private bool HandleUserRequestEquipTeamItem(NetworkCommunicator peer, UserRequestEquipTeamItem message)
    {
        BattleSideEnum team = peer.GetComponent<MissionPeer>()?.Team?.Side ?? BattleSideEnum.None;

        // if equipping and slot already has an item, return it first
        if (message.Equip && PendingEquipment.TryGetValue(peer, out var pending) && pending.TryGetValue(message.Slot, out string? existingItemId))
        {
            if (!string.IsNullOrEmpty(existingItemId) && existingItemId != message.Item)
            {
                if (TryEquipItem(team, existingItemId, false, message.Slot))
                {
                    var existingTeamItem = FindItem(team, existingItemId);
                    if (existingTeamItem != null)
                    {
                        BroadcastToTeam(team, new ServerSendTeamItemQuantityUpdated { Peer = peer.VirtualPlayer, Item = existingItemId, Quantity = existingTeamItem.Quantity });
                        SendPeerEquipResult(peer, existingItemId, false, message.Slot, existingTeamItem.Quantity);
                    }
                }

                SetItemReturnedByPeer(peer, message.Slot);
            }
        }

        if (!TryEquipItem(team, message.Item, message.Equip, message.Slot))
        {
            return false;
        }

        var teamItem = FindItem(team, message.Item);
        if (teamItem == null)
        {
            return false;
        }

        if (message.Equip)
        {
            SetItemPendingForPeer(peer, teamItem.Id, message.Slot);
        }
        else
        {
            SetItemReturnedByPeer(peer, message.Slot);
        }

        BroadcastToTeam(team, new ServerSendTeamItemQuantityUpdated { Peer = peer.VirtualPlayer, Item = message.Item, Quantity = teamItem.Quantity });
        SendPeerEquipResult(peer, message.Item, message.Equip, message.Slot, teamItem.Quantity);
        return true;
    }

    private bool HandleUserRequestReEquipTeamItems(NetworkCommunicator peer, UserRequestReEquipTeamItems message)
    {
        TryReEquipLastUsedEquipment(peer);
        return true;
    }

    private bool HandleUserRequestGetTeamItemsList(NetworkCommunicator peer, UserRequestGetTeamItemsList message)
    {
        SendPeerTeamItemsList(peer);
        return true;
    }

    private bool HandleUserRequestSpawnWithTeamEquipment(NetworkCommunicator peer, UserRequestSpawnWithTeamEquipment message)
    {
        if (!PendingEquipment.TryGetValue(peer, out Dictionary<CrpgItemSlot, string>? pending) || pending.Count == 0)
        {
            return false;
        }

        ReadyToSpawn.Add(peer);
        return true;
    }
}

using Crpg.Module.Api.Models.Users;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common.FriendlyFireReport;

internal class FriendlyFireReportServerBehavior : MissionNetwork
{
    private class TeamHitRecord
    {
        public DateTime Time { get; set; }
        public NetworkCommunicator Victim { get; set; }
        public float Damage { get; set; }
        public string WeaponName { get; set; }
        public bool WasReported { get; set; }
        public bool HasDecayed { get; set; }

        public TeamHitRecord(DateTime time, NetworkCommunicator victim, float damage, bool wasReported, bool hasDecayed = false, string weaponName = "unknown")
        {
            Time = time;
            Victim = victim;
            Damage = damage;
            WeaponName = weaponName;
            WasReported = wasReported;
            HasDecayed = hasDecayed;
        }

        public bool CheckAndMarkDecay(DateTime now, double decaySeconds)
        {
            if (!HasDecayed && (now - Time).TotalSeconds > decaySeconds)
            {
                HasDecayed = true;
            }

            return HasDecayed;
        }
    }

    // Track hit info of all teamhits
    private readonly Dictionary<NetworkCommunicator, List<TeamHitRecord>> _teamHitHistory = new();
    // Track which peer last team-damaged a specific peer
    private readonly Dictionary<NetworkCommunicator, NetworkCommunicator> _lastTeamHitBy = new();
    private MultiplayerRoundController? _roundController;
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void AfterStart()
    {
        _roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();
        if (_roundController != null)
        {
            _roundController.OnRoundStarted += OnRoundStarted;
        }
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        if (_roundController != null)
        {
            _roundController.OnRoundStarted -= OnRoundStarted;
        }
    }

    public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
    {
        base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);

        if (!CrpgServerConfiguration.IsFriendlyFireReportEnabled)
        {
            return; // If control M reporting is disabled, do not process team hits
        }

        if (Mission.Current?.GetMissionBehavior<MultiplayerWarmupComponent>()?.IsInWarmup ?? false)
        {
            return; // Still in warmup phase
        }

        if (affectedAgent == null || affectorAgent == null || affectedAgent == affectorAgent)
        {
            return;
        }

        if (affectedAgent.IsMount) // Check if victim a mount
        {
            if (affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.IsActive())
            {
                affectedAgent = affectedAgent.RiderAgent; // use the rider as the victim
            }
            else
            {
                return; // ignore hits to mounts without riders
            }
        }

        if (affectorAgent.IsMount) // Check if the attacker agent is a mount
        {
            if (affectorAgent.RiderAgent != null && affectorAgent.RiderAgent.IsActive())
            {
                affectorAgent = affectorAgent.RiderAgent; // use the rider as the attacker
            }
            else
            {
                return; // ignore hits from mounts without riders
            }
        }

        // Check if both agents are player controlled, use rider (set above) if mount was the attacker or victim
        if (!affectedAgent.IsPlayerControlled || !affectorAgent.IsPlayerControlled)
        {
            return;
        }

        // Same team
        if (affectedAgent.Team?.TeamIndex != affectorAgent.Team?.TeamIndex)
        {
            return; // not a team hit
        }

        NetworkCommunicator? affectorNetworkPeer = affectorAgent.MissionPeer?.GetNetworkPeer(); // attacker
        NetworkCommunicator? affectedNetworkPeer = affectedAgent.MissionPeer?.GetNetworkPeer(); // victim

        if (affectorNetworkPeer == null || affectedNetworkPeer == null || affectorNetworkPeer == affectedNetworkPeer)
        {
            return; // No network peers available or same peer
        }

        string weaponName = "unknown";
        if (!affectorWeapon.IsEmpty && !affectorWeapon.IsEqualTo(MissionWeapon.Invalid) && affectorWeapon.Item != null)
        {
            weaponName = affectorWeapon.Item.Name.ToString();
        }

        if (!_teamHitHistory.TryGetValue(affectorNetworkPeer, out var hitList))
        {
            hitList = new List<TeamHitRecord>();
            _teamHitHistory[affectorNetworkPeer] = hitList;
        }

        hitList.Add(new TeamHitRecord(DateTime.UtcNow, affectedNetworkPeer, attackCollisionData.InflictedDamage, false, false, weaponName));

        // Track last team hitter for the affected agent
        _lastTeamHitBy[affectedNetworkPeer] = affectorNetworkPeer;

        if (GameNetwork.IsServer)
        {
            GameNetwork.BeginModuleEventAsServer(affectedNetworkPeer);
            GameNetwork.WriteMessage(new FriendlyFireHitMessage(affectorAgent.Index, attackCollisionData.InflictedDamage, CrpgServerConfiguration.FriendlyFireReportWindowSeconds));
            GameNetwork.EndModuleEventAsServer();
        }
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
    }

    public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
    {
        base.OnPlayerDisconnectedFromServer(networkPeer);

        // Remove hits where this peer was the attacker
        _teamHitHistory.Remove(networkPeer);

        // Remove hits where this peer was the victim inside other players' hit records
        foreach (var hitList in _teamHitHistory.Values)
        {
            hitList.RemoveAll(hit => hit.Victim == networkPeer);
        }

        // remove entries where this peer was the victim
        _lastTeamHitBy.Remove(networkPeer);

        // remove entries where this peer was the attacker
        var victims = _lastTeamHitBy
            .Where(kvp => kvp.Value == networkPeer)
            .Select(kvp => kvp.Key)
            .ToList();
        foreach (var victimPeer in victims)
        {
            // SendClientDisplayMessage(victimPeer, $"[FF] Your last teamhit attacker {networkPeer.UserName} has left the match.", FriendlyFireMessageMode.TeamDamageReportError);
            _lastTeamHitBy.Remove(victimPeer);
        }
    }

    public (int activeReported, int decayedReported, int notReported) GetReportedTeamHitBreakdown(NetworkCommunicator peer, bool detailed = true)
    {
        if (peer == null || !_teamHitHistory.TryGetValue(peer, out var hitList))
        {
            return (0, 0, 0);
        }

        int activeReported = 0;
        int decayedReported = 0;
        int notReported = 0;
        DateTime now = DateTime.UtcNow;

        foreach (var hit in hitList)
        {
            if (!hit.WasReported)
            {
                if (detailed)
                {
                    notReported++;
                }

                continue;
            }

            hit.CheckAndMarkDecay(now, CrpgServerConfiguration.FriendlyFireReportDecaySeconds);

            if (hit.HasDecayed)
            {
                if (detailed)
                {
                    decayedReported++;
                }
            }
            else
            {
                activeReported++;
            }
        }

        return (activeReported, decayedReported, notReported);
    }

    public int CountTotalHitsAgainstVictim(NetworkCommunicator attacker, NetworkCommunicator victim)
    {
        if (!_teamHitHistory.TryGetValue(attacker, out var hits))
        {
            return 0;
        }

        return hits.Count(hit => hit.Victim == victim);
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsServer)
        {
            base.AddRemoveMessageHandlers(registerer);
            registerer.Register<FriendlyFireReportClientMessage>((peer, message) =>
            {
                OnFriendlyFireReportRecieved(peer, message);
                return true;
            });
        }
    }

    private void OnRoundStarted()
    {
        _lastTeamHitBy.Clear(); // clear last person to hit a peer

        // mark all reported hits as decayed on round start but preserve data
        if (CrpgServerConfiguration.IsFriendlyFireReportDecayOnRoundStartEnabled)
        {
            foreach (var hitList in _teamHitHistory.Values)
            {
                foreach (var hit in hitList)
                {
                    if (hit.WasReported)
                    {
                        hit.HasDecayed = true;
                    }
                }
            }
        }
    }

    private void OnFriendlyFireReportRecieved(NetworkCommunicator peer, FriendlyFireReportClientMessage message)
    {
        if (!CrpgServerConfiguration.IsFriendlyFireReportEnabled)
        {
            if (peer != null && peer.IsConnectionActive)
            {
                SendClientDisplayMessage(peer, "[FF] Ctrl+M reporting is currently disabled on this server.", FriendlyFireMessageMode.TeamDamageReportError);
            }

            return; // If control M reporting is disabled, do not process team hit reports
        }

        if (peer == null || !peer.IsConnectionActive)
        {
            return;
        }

        if (Mission.Current?.GetMissionBehavior<MultiplayerWarmupComponent>()?.IsInWarmup ?? false)
        {
            return; // Still in warmup phase
        }

        if (!_lastTeamHitBy.TryGetValue(peer, out NetworkCommunicator? attackingPeer))
        {
            return; // No record of a team hit
        }

        if (attackingPeer == null || !attackingPeer.IsConnectionActive)
        {
            SendClientDisplayMessage(peer, "[FF] No connected attacker peer found for your report.", FriendlyFireMessageMode.TeamDamageReportError);
            return; // Attacker is not active
        }

        // Mark the last hit as reported
        var recentHit = _teamHitHistory[attackingPeer].LastOrDefault(h =>
            h.Victim == peer &&
            !h.WasReported &&
            (CrpgServerConfiguration.FriendlyFireReportWindowSeconds == 0 ||
            (DateTime.UtcNow - h.Time).TotalSeconds < CrpgServerConfiguration.FriendlyFireReportWindowSeconds));

        if (recentHit != null)
        {
            recentHit.WasReported = true;
        }
        else
        {
            SendClientDisplayMessage(peer, "[FF] No recent team hit found to report.", FriendlyFireMessageMode.TeamDamageReportError);
            return; // No recent unreported hit found
        }

        // Get Teamhits by attacker
        var (countActive, countDecayed, countNotReported) = GetReportedTeamHitBreakdown(attackingPeer);
        int countAttacksOnVictim = CountTotalHitsAgainstVictim(attackingPeer, peer);
        int maxHits = CrpgServerConfiguration.FriendlyFireReportMaxHits;

        // Notify the attacker about the report
        SendClientDisplayMessage(attackingPeer, $"[FF] {peer.UserName} reported your team hit (Dmg: {recentHit.Damage}). {countActive}/{maxHits} team hits until getting kicked.", FriendlyFireMessageMode.TeamDamageReportForAttacker);

        // Notify the victim about the report
        SendClientDisplayMessage(peer, $"[FF] Reported {attackingPeer.UserName} for team hit (Dmg: {recentHit.Damage}).");

        // Notify all admins about the report
        if (CrpgServerConfiguration.IsFriendlyFireReportNotifyAdminsEnabled)
        {
            SendClientDisplayMessageToAdmins($"[FF] {attackingPeer.UserName} reported by {peer.UserName}. [{countActive}/{maxHits}] decayed: {countDecayed} not reported: {countNotReported} on reporter: {countAttacksOnVictim}");
        }

        // Logging
        TryLogFriendlyFire(peer, attackingPeer, countActive, countDecayed, countNotReported, countAttacksOnVictim, (int)recentHit.Damage, recentHit.WeaponName);

        if (countActive >= maxHits)
        {
            SendClientDisplayMessage(attackingPeer, $"[FF] You have been kicked for excessive team hits ({maxHits})", FriendlyFireMessageMode.TeamDamageReportKick);
            SendClientDisplayMessageToAllExcept(attackingPeer, $"[FF] {attackingPeer.UserName} has been kicked for excessive team hits ({maxHits}).", FriendlyFireMessageMode.TeamDamageReportKick);

            TryLogKickedForFriendlyFire(attackingPeer, countActive, countDecayed, countNotReported);

            KickHelper.Kick(attackingPeer, DisconnectType.KickedDueToFriendlyDamage);
        }
    }

    private void TryLogFriendlyFire(NetworkCommunicator peer, NetworkCommunicator attackingPeer, int reportedHits, int decayedHits, int unreportedHits, int onReporterHits, int damage, string weaponName)
    {
        var peerComponent = peer?.GetComponent<CrpgPeer>();
        var attackerComponent = attackingPeer?.GetComponent<CrpgPeer>();

        if (peerComponent?.User == null || attackerComponent?.User == null)
        {
            return;
        }

        int peerUserId = peerComponent.User.Id;
        int attackerUserId = attackerComponent.User.Id;

        Mission.Current?.GetMissionBehavior<CrpgActivityLogsBehavior>()
            ?.AddTeamHitReportedLogWrapper(peerUserId, attackerUserId, reportedHits, decayedHits, unreportedHits, onReporterHits, damage, weaponName);
    }

    private void TryLogKickedForFriendlyFire(NetworkCommunicator peer, int reportedHits, int decayedHits, int unreportedHits)
    {
        var peerComponent = peer?.GetComponent<CrpgPeer>();

        if (peerComponent?.User == null)
        {
            return;
        }

        int peerUserId = peerComponent.User.Id;

        Mission.Current?.GetMissionBehavior<CrpgActivityLogsBehavior>()
            ?.AddTeamHitReportedUserKickedLogWrapper(peerUserId, reportedHits, decayedHits, unreportedHits);
    }

    private void SendClientDisplayMessage(NetworkCommunicator peer, string displayText, FriendlyFireMessageMode messageMode = FriendlyFireMessageMode.Default)
    {
        if (peer == null || !peer.IsConnectionActive)
        {
            return;
        }

        if (!Enum.IsDefined(typeof(FriendlyFireMessageMode), messageMode))
        {
            messageMode = FriendlyFireMessageMode.Default; // fallback to safe default
        }

        GameNetwork.BeginModuleEventAsServer(peer);
        GameNetwork.WriteMessage(new FriendlyFireNotificationMessage(displayText, messageMode));
        GameNetwork.EndModuleEventAsServer();
    }

    private void SendClientDisplayMessageToAllExcept(NetworkCommunicator excludedPeer, string displayText, FriendlyFireMessageMode messageMode = FriendlyFireMessageMode.Default)
    {
        foreach (var peer in GameNetwork.NetworkPeers)
        {
            if (peer.IsConnectionActive && peer != excludedPeer)
            {
                SendClientDisplayMessage(peer, displayText, messageMode);
            }
        }
    }

    private void SendClientDisplayMessageToAdmins(string displayText)
    {
        foreach (var peer in GameNetwork.NetworkPeers)
        {
            if (peer.IsConnectionActive)
            {
                var crpgUser = peer.GetComponent<CrpgPeer>()?.User;
                if (crpgUser != null && crpgUser.Role is CrpgUserRole.Moderator or CrpgUserRole.Admin)
                {
                    SendClientDisplayMessage(peer, displayText, FriendlyFireMessageMode.TeamDamageReportForAdmins);
                }
            }
        }
    }
}

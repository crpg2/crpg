using Crpg.Module.Api.Models.Users;
using Crpg.Module.Modes.Battle;
using Crpg.Module.Modes.Conquest;
using Crpg.Module.Modes.Dtv;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
    private MultiplayerWarmupComponent? _warmupComponent;
    private CrpgDtvServer? _crpgDtvServer;
    private CrpgBattleServer? _crpgBattleServer;
    private CrpgConquestServer? _crpgConquestServer;

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
    }

    public override void AfterStart()
    {
        _crpgBattleServer = Mission.Current.GetMissionBehavior<CrpgBattleServer>();
        _crpgDtvServer = Mission.Current.GetMissionBehavior<CrpgDtvServer>();
        _crpgConquestServer = Mission.Current.GetMissionBehavior<CrpgConquestServer>();

        if (_crpgBattleServer != null)
        {
            _roundController = Mission.Current.GetMissionBehavior<MultiplayerRoundController>();

            if (_roundController != null)
            {
                _roundController.OnRoundStarted += OnRoundControllerRoundStarted;
            }
        }

        if (_crpgDtvServer != null)
        {
            _crpgDtvServer.DtvRoundStarted += OnDtvRoundStarted;
        }

        if (_crpgConquestServer != null)
        {
            _crpgConquestServer.ConquestStageStarted += OnConquestStageStarted;
        }

        _warmupComponent = Mission.Current?.GetMissionBehavior<MultiplayerWarmupComponent>();
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        if (_roundController != null)
        {
            _roundController.OnRoundStarted -= OnRoundControllerRoundStarted;
        }

        if (_crpgDtvServer != null)
        {
            _crpgDtvServer.DtvRoundStarted -= OnDtvRoundStarted;
        }

        if (_crpgConquestServer != null)
        {
            _crpgConquestServer.ConquestStageStarted -= OnConquestStageStarted;
        }
    }

    public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
    {
        base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);

        if (!CrpgServerConfiguration.IsFriendlyFireReportEnabled)
        {
            return; // If control M reporting is disabled, do not process team hits
        }

        if (_warmupComponent != null && _warmupComponent.IsInWarmup)
        {
            return; // Still in warmup phase
        }

        if (_crpgBattleServer != null)
        {
            if (_roundController != null && _roundController.CurrentRoundState != MultiplayerRoundState.InProgress)
            {
                return;
            }
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

        // Same team
        if (affectorAgent.Team == null || affectorAgent.Team.Side == BattleSideEnum.None || affectorAgent.Team != affectedAgent.Team)
        {
            return; // same team checks
        }

        // Check if both agents are player controlled, use rider (set above) if mount was the attacker or victim
        if (!affectedAgent.IsPlayerControlled || !affectorAgent.IsPlayerControlled)
        {
            return;
        }

        if (attackCollisionData.InflictedDamage <= 0 || attackCollisionData.AttackBlockedWithShield)
        {
            return; // blocked attack or shield
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

    private void OnDtvRoundStarted(int roundNumber)
    {
        if (CrpgServerConfiguration.IsFriendlyFireReportDecayOnRoundStartEnabled)
        {
            DecayReportedHits();
            _lastTeamHitBy.Clear();
        }
    }

    private void OnConquestStageStarted(int stageIndex)
    {
        if (stageIndex == 0 && CrpgServerConfiguration.IsFriendlyFireReportDecayOnRoundStartEnabled)
        {
            DecayReportedHits();
            _lastTeamHitBy.Clear();
        }
    }

    private void OnRoundControllerRoundStarted()
    {
        if (CrpgServerConfiguration.IsFriendlyFireReportDecayOnRoundStartEnabled)
        {
            DecayReportedHits();
            _lastTeamHitBy.Clear();
        }
    }

    private void DecayReportedHits()
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

    private void OnFriendlyFireReportRecieved(NetworkCommunicator peer, FriendlyFireReportClientMessage message)
    {
        if (!CrpgServerConfiguration.IsFriendlyFireReportEnabled)
        {
            if (peer != null && peer.IsConnectionActive)
            {
                // SendClientDisplayMessage(peer, "[FF] Friendly fire reporting is currently disabled on this server.", FriendlyFireMessageMode.TeamDamageReportError);
                // TextObject reportingDisabledText = GameTexts.FindText("str_ff_teamhit_msg_reporting_disabled");
                TextObject reportingDisabledText = new("{=Chi61w9L}Friendly fire reporting is currently disabled on this server.");
                SendClientDisplayMessage(peer, reportingDisabledText.ToString(), FriendlyFireMessageMode.TeamDamageReportError);
            }

            return; // If control M reporting is disabled, do not process team hit reports
        }

        if (peer == null || !peer.IsConnectionActive)
        {
            return;
        }

        if (_warmupComponent != null && _warmupComponent.IsInWarmup)
        {
            return; // Still in warmup phase
        }

        if (!_lastTeamHitBy.TryGetValue(peer, out NetworkCommunicator? attackingPeer))
        {
            return; // No record of a team hit
        }

        if (attackingPeer == null || !attackingPeer.IsConnectionActive)
        {
            // SendClientDisplayMessage(peer, "[FF] No connected attacker peer found for your report.", FriendlyFireMessageMode.TeamDamageReportError);
            // TextObject noPeerFoundText = GameTexts.FindText("str_ff_teamhit_msg_no_peer_found");
            TextObject noPeerFoundText = new("{=xq7QtuZ0}No connected attacker peer found for your report.");
            SendClientDisplayMessage(peer, noPeerFoundText.ToString(), FriendlyFireMessageMode.TeamDamageReportError);
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
            // SendClientDisplayMessage(peer, "[FF] No recent team hit found to report.", FriendlyFireMessageMode.TeamDamageReportError);
            // TextObject noRecentHitText = GameTexts.FindText("str_ff_teamhit_msg_no_hit_found");
            TextObject noRecentHitText = new("{=zD8mGERY}No recent team hit found to report.");
            SendClientDisplayMessage(peer, noRecentHitText.ToString(), FriendlyFireMessageMode.TeamDamageReportError);
            return; // No recent unreported hit found
        }

        // Get Teamhits by attacker
        var (countActive, countDecayed, countNotReported) = GetReportedTeamHitBreakdown(attackingPeer);
        int countAttacksOnVictim = CountTotalHitsAgainstVictim(attackingPeer, peer);
        int maxHits = CrpgServerConfiguration.FriendlyFireReportMaxHits;

        // Notify the attacker about the report
        // SendClientDisplayMessage(attackingPeer, $"[FF] {peer.UserName} reported your team hit (Dmg: {recentHit.Damage}). {countActive}/{maxHits} team hits until getting kicked.", FriendlyFireMessageMode.TeamDamageReportForAttacker);
        // TextObject notifyAttackerText = GameTexts.FindText("str_ff_teamhit_msg_notify_attacker");
        TextObject notifyAttackerText = new("{=S4FzPA70}[FF] {VICTIM} reported your team hit. (Dmg: {DAMAGE}) {HITSACTIVE}/{HITSMAX} team hits until kick.");
        notifyAttackerText.SetTextVariable("VICTIM", peer.UserName);
        notifyAttackerText.SetTextVariable("DAMAGE", recentHit.Damage);
        notifyAttackerText.SetTextVariable("HITSACTIVE", countActive);
        notifyAttackerText.SetTextVariable("HITSMAX", maxHits);
        SendClientDisplayMessage(attackingPeer, notifyAttackerText.ToString(), FriendlyFireMessageMode.TeamDamageReportForAttacker);

        // Notify the victim about the report
        // SendClientDisplayMessage(peer, $"[FF] Reported {attackingPeer.UserName} for team hit (Dmg: {recentHit.Damage}).");
        // TextObject notifyVictimText = GameTexts.FindText("str_ff_teamhit_msg_notify_victim");
        TextObject notifyVictimText = new("{=1Vw2P1hD}Reported {ATTACKER} for team hit (Dmg: {DAMAGE}).");
        notifyVictimText.SetTextVariable("ATTACKER", attackingPeer.UserName);
        notifyVictimText.SetTextVariable("DAMAGE", recentHit.Damage);
        SendClientDisplayMessage(peer, notifyVictimText.ToString(), FriendlyFireMessageMode.TeamDamageReportForVictim);

        // Notify all admins about the report
        if (CrpgServerConfiguration.IsFriendlyFireReportNotifyAdminsEnabled)
        {
            // SendClientDisplayMessageToAdmins($"[FF] {attackingPeer.UserName} reported by {peer.UserName}. [{countActive}/{maxHits}] decayed: {countDecayed} not reported: {countNotReported} on reporter: {countAttacksOnVictim}");
            // TextObject notifyAdminsText = GameTexts.FindText("str_ff_teamhit_msg_notify_admins");
            TextObject notifyAdminsText = new("{=vNmyS3cU}{ATTACKER} reported by {VICTIM}. [{HITSACTIVE}/{HITSMAX}] decayed: {HITSDECAYED} not reported: {HITSNOTREPORTED} on reporter: {HITSONVICTIM}");
            notifyAdminsText.SetTextVariable("ATTACKER", attackingPeer.UserName);
            notifyAdminsText.SetTextVariable("VICTIM", peer.UserName);
            notifyAdminsText.SetTextVariable("HITSACTIVE", countActive);
            notifyAdminsText.SetTextVariable("HITSMAX", maxHits);
            notifyAdminsText.SetTextVariable("HITSDECAYED", countDecayed);
            notifyAdminsText.SetTextVariable("HITSNOTREPORTED", countNotReported);
            notifyAdminsText.SetTextVariable("HITSONVICTIM", countAttacksOnVictim);
            SendClientDisplayMessageToAdmins(notifyAdminsText.ToString());
        }

        // Logging
        TryLogFriendlyFire(peer, attackingPeer, countActive, countDecayed, countNotReported, countAttacksOnVictim, (int)recentHit.Damage, recentHit.WeaponName);

        if (countActive >= maxHits)
        {
            // SendClientDisplayMessage(attackingPeer, $"[FF] You have been kicked for excessive team hits ({maxHits})", FriendlyFireMessageMode.TeamDamageReportKick);
            // TextObject notifyAttackerKickText = GameTexts.FindText("str_ff_teamhit_msg_kick_notify_attacker");
            TextObject notifyAttackerKickText = new("{=uPwSqhuA}You have been kicked for excessive team hits ({HITSMAX})");
            notifyAttackerKickText.SetTextVariable("HITSMAX", maxHits);
            SendClientDisplayMessage(attackingPeer, notifyAttackerKickText.ToString(), FriendlyFireMessageMode.TeamDamageReportKick);

            // SendClientDisplayMessageToAllExcept(attackingPeer, $"[FF] {attackingPeer.UserName} has been kicked for excessive team hits ({maxHits}).", FriendlyFireMessageMode.TeamDamageReportKick);
            // TextObject notifyOthersKickText = GameTexts.FindText("str_ff_teamhit_msg_kick_notify_others");
            TextObject notifyOthersKickText = new("{=MdIKl2Bv}{ATTACKER} has been kicked for excessive team hits ({HITSMAX}).");
            notifyOthersKickText.SetTextVariable("ATTACKER", attackingPeer.UserName);
            notifyOthersKickText.SetTextVariable("HITSMAX", maxHits);
            SendClientDisplayMessageToAllExcept(attackingPeer, notifyOthersKickText.ToString(), FriendlyFireMessageMode.TeamDamageReportKick);

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

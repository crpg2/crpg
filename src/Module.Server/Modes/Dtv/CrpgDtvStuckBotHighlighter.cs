using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvStuckBotHighlighter : MissionLogic
{
    /// <summary>How long after the last bot death should the bots be highlighted.</summary>
    private const float HighlightDelay = 30f;

    /// <summary>The minimum wave age in seconds to start counting down before highlighting bots.</summary>
    private const float MinWaveAge = 60f;

    private readonly CrpgDtvClient _dtvClient;
    private MissionTime _lastAttackerBotKillTime;
    private MissionTime _waveStartTime;
    private bool _botsHighlighted;
    private bool _waveActive;

    public CrpgDtvStuckBotHighlighter(CrpgDtvClient dtvClient)
    {
        _dtvClient = dtvClient;
    }

    public override void OnBehaviorInitialize()
    {
        _dtvClient.OnRoundStart += OnRoundStart;
        _dtvClient.OnWaveStart += OnWaveStart;
    }

    public override void OnRemoveBehavior()
    {
        _dtvClient.OnRoundStart -= OnRoundStart;
        _dtvClient.OnWaveStart -= OnWaveStart;
        base.OnRemoveBehavior();
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        if (!_botsHighlighted
            && _waveActive
            && _waveStartTime.ElapsedSeconds >= MinWaveAge + HighlightDelay
            && _lastAttackerBotKillTime.ElapsedSeconds >= HighlightDelay
            && Mission.AttackerTeam?.ActiveAgents.Count > 0)
        {
            ApplyHighlights();
        }
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        if (_waveActive && !affectedAgent.IsPlayerControlled && affectedAgent.Team?.Side == BattleSideEnum.Attacker)
        {
            _lastAttackerBotKillTime = MissionTime.Now;
            ClearHighlights();
        }
    }

    private void OnRoundStart()
    {
        _waveActive = false;
        ClearHighlights();
    }

    private void OnWaveStart()
    {
        _waveActive = true;
        _waveStartTime = MissionTime.Now;
        _lastAttackerBotKillTime = MissionTime.Now;
        ClearHighlights();
    }

    private void ApplyHighlights()
    {
        _botsHighlighted = true;
        uint contourColor = new Color(1f, 0.27f, 0.27f).ToUnsignedInteger();
        foreach (Agent agent in Mission.AttackerTeam.ActiveAgents)
        {
            if (!agent.IsPlayerControlled)
            {
                agent.AgentVisuals?.SetContourColor(contourColor);
            }
        }
    }

    private void ClearHighlights()
    {
        if (!_botsHighlighted)
        {
            return;
        }

        _botsHighlighted = false;
        foreach (Agent agent in Mission.AttackerTeam.TeamAgents)
        {
            if (!agent.IsPlayerControlled)
            {
                agent.AgentVisuals?.SetContourColor(null);
            }
        }
    }
}

using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvStuckBotHighlighter : MissionLogic
{
    /// <summary>How long after the last bot death should the bots be highlighted.</summary>
    private const float HighlightDelay = 30f;

    /// <summary>The minimum wave age in seconds to start counting down before highlighting bots.</summary>
    private const float MinWaveAge = 60f;

    /// <summary>The maximum number of alive bots to start highlighting.</summary>
    private const int MaxBotsToHighlight = 5;

    private readonly CrpgDtvClient _dtvClient;
    private readonly List<Agent> _highlightedAgents = new();
    private MissionTime _lastAttackerBotKillTime;
    private MissionTime _waveStartTime;
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

        int activeBotCount = Mission.AttackerTeam?.ActiveAgents.Count(a => !a.IsPlayerControlled) ?? 0;
        if (_highlightedAgents.Count == 0
            && _waveActive
            && _waveStartTime.ElapsedSeconds >= MinWaveAge + HighlightDelay
            && _lastAttackerBotKillTime.ElapsedSeconds >= HighlightDelay
            && activeBotCount > 0
            && activeBotCount < MaxBotsToHighlight)
        {
            ApplyHighlights();
        }
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
    {
        if (_waveActive && !affectedAgent.IsPlayerControlled && affectedAgent.Team?.Side == BattleSideEnum.Attacker)
        {
            _lastAttackerBotKillTime = MissionTime.Now;
            _highlightedAgents.Remove(affectedAgent);
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
        uint contourColor = new Color(1f, 0.27f, 0.27f).ToUnsignedInteger();
        foreach (Agent agent in Mission.AttackerTeam.ActiveAgents)
        {
            if (!agent.IsPlayerControlled)
            {
                agent.AgentVisuals?.SetContourColor(contourColor);
                _highlightedAgents.Add(agent);
            }
        }

        InformationManager.DisplayMessage(new InformationMessage
        {
            Information = new TextObject("{=Ht7vJBcR}Remaining enemies are now highlighted!").ToString(),
            Color = new Color(1f, 0.27f, 0.27f),
        });
    }

    private void ClearHighlights()
    {
        foreach (Agent agent in _highlightedAgents)
        {
            if (agent.IsActive())
            {
                agent.AgentVisuals?.SetContourColor(null);
            }
        }

        _highlightedAgents.Clear();
    }
}

using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class KillStuckBotBehavior : MissionLogic
{
    private const float StuckDurationLimit = 30f;

    private readonly Func<Mission, Agent, bool>? _agentFilter;
    private readonly Dictionary<int, (Vec3 pos, float time)> _lastMoves;
    private MissionTimer? _checkTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KillStuckBotBehavior"/> class.
    /// </summary>
    /// <param name="agentFilter">Agents that should be monitored.</param>
    public KillStuckBotBehavior(Func<Mission, Agent, bool>? agentFilter)
    {
        _agentFilter = agentFilter;
        _lastMoves = [];
    }

    public override void OnMissionTick(float dt)
    {
        _checkTimer ??= new MissionTimer(duration: 1f);
        if (!_checkTimer.Check(reset: true))
        {
            return;
        }

        List<Agent>? agentsToKill = null;
        foreach (var agent in Mission.Current.Agents)
        {
            if (!agent.IsAIControlled || agent.State != AgentState.Active)
            {
                continue;
            }

            if (_agentFilter != null && !_agentFilter(Mission, agent))
            {
                continue;
            }

            if (!_lastMoves.TryGetValue(agent.Index, out var lastMove)
                || !lastMove.pos.NearlyEquals(agent.Position, epsilon: 0.1f))
            {
                _lastMoves[agent.Index] = (agent.Position, Mission.CurrentTime);
                continue;
            }

            if (Mission.CurrentTime - lastMove.time > StuckDurationLimit)
            {
                // Kill the agents in a second loop to avoid modifying the enumerator being iterated.
                (agentsToKill ??= []).Add(agent);
            }
        }

        if (agentsToKill != null)
        {
            foreach (var agent in agentsToKill)
            {
                Debug.Print($"Killing bot {agent.Index} ({agent.Name}) that was stuck for more than {StuckDurationLimit} seconds");
                agent.Die(new Blow(agent.Index)
                {
                    DamageType = DamageTypes.Invalid,
                    BaseMagnitude = 10000f,
                    GlobalPosition = agent.Position,
                    DamagedPercentage = 1f,
                }, Agent.KillInfo.Gravity);
            }
        }
    }

    public override void OnClearScene()
    {
        _lastMoves.Clear();
    }

    public override void OnAgentRemoved(
        Agent affectedAgent,
        Agent affectorAgent,
        AgentState agentState,
        KillingBlow blow)
    {
        _lastMoves.Remove(affectedAgent.Index);
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using Crpg.Module.Helpers;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.AiComponents;
public class DtvAiComponent : CommonAIComponent
{
    private const int ViscountTargetTimerDuration = 30;
    private MissionTimer? _targetTimer;
    private MissionTimer? _tickOccasionally;
    public DtvAiComponent(Agent agent)
        : base(agent)
    {
    }

    public override void Initialize() // Not being called automatically when the component is instantiated
    {
        _targetTimer = new(ViscountTargetTimerDuration * 2);
    }

    public override void OnTickAsAI(float dt)
    {
        _tickOccasionally ??= new(1f);
        if (_tickOccasionally.Check(true))
        {
            TickOccasionally();
        }
    }

    public override void OnHit(Agent affectorAgent, int damage, in MissionWeapon affectorWeapon)
    {
        if (affectorAgent.Team == Mission.Current.Teams.Defender)
        {
            ResetTargetTimer();
        }
    }

    public void ResetTargetTimer()
    {
        _targetTimer = null;
        Agent.SetAutomaticTargetSelection(true);
    }

    private void TickOccasionally()
    {
        CheckTargetTimer();
    }

    private void CheckTargetTimer()
    {
        _targetTimer ??= new(MathHelper.RandomWithVariance(ViscountTargetTimerDuration, 0.2f));
        if (_targetTimer.Check(true))
        {
            FocusVip();
        }
    }

    private void FocusVip()
    {
        var agents = Mission.Current.Agents.ToList();
        foreach (Agent agent in agents)
        {
            if (agent?.Origin?.Troop?.StringId != null && agent.Origin.Troop.StringId.StartsWith("crpg_dtv_vip_"))
            {
                Agent.SetAutomaticTargetSelection(false);
                Agent.SetTargetAgent(agent);
                break;
            }
        }
    }
}

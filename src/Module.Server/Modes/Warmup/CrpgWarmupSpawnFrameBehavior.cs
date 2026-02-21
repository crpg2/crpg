using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.Warmup;
public class CrpgWarmupSpawnFrameBehavior : SpawnFrameBehaviorBase
{
    public override void Initialize()
    {
        base.Initialize();
    }

    public override MatrixFrame GetSpawnFrame(Team team, bool hasMount, bool isInitialSpawn)
    {
        List<GameEntity> spawnPoints = SpawnPoints.ToList();

        return GetSpawnFrameFromSpawnPoints(spawnPoints, team, hasMount);
    }

    private new MatrixFrame GetSpawnFrameFromSpawnPoints(IList<GameEntity> spawnPointsList, Team team, bool hasMount)
    {
        float highScore = float.MinValue;
        int index = -1;
        for (int i = 0; i < spawnPointsList.Count; i++)
        {
            float score = MBRandom.RandomFloat * 2f;
            float proximityScore = 0f;
            if (hasMount && spawnPointsList[i].HasTag("exclude_mounted"))
            {
                score -= 1000f;
            }

            if (!hasMount && spawnPointsList[i].HasTag("exclude_footmen"))
            {
                score -= 1000f;
            }

            foreach (Agent agent in Mission.Current.Agents)
            {
                if (agent.IsMount)
                {
                    continue;
                }

                float distance = (agent.Position - spawnPointsList[i].GlobalPosition).Length;
                float influence = 3.0f - distance * 0.15f;
                proximityScore += influence;
            }

            if (proximityScore > 0f)
            {
                proximityScore /= (float)Mission.Current.Agents.Count;
            }

            score += proximityScore;
            if (score > highScore)
            {
                highScore = score;
                index = i;
            }
        }

        MatrixFrame globalFrame = spawnPointsList[index].GetGlobalFrame();
        globalFrame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
        return globalFrame;
    }
}

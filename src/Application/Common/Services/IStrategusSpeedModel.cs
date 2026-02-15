using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Terrains;

namespace Crpg.Application.Common.Services;

/// <summary>
/// Service to compute the speed of a <see cref="Party"/>.
/// </summary>
internal interface IStrategusSpeedModel
{
    /// <summary>Compute the Party Speed with detailed breakdown.</summary>
    PartySpeed ComputePartySpeed(Party party, TerrainType? currentTerrainType = null);
}

internal class StrategusSpeedModel(IStrategusRouting strategusRouting) : IStrategusSpeedModel
{
    private readonly IStrategusRouting _strategusRouting = strategusRouting;

    private readonly double baseSpeed = 1; // TODO: tune https://github.com/verdie-g/crpg/issues/195
    private readonly double weightFactor = 1;
    private readonly double forcedMarchSpeed = 2;

    /// <inheritdoc />
    public PartySpeed ComputePartySpeed(Party party, TerrainType? currentTerrainType)
    {
        double terrainSpeedFactor = currentTerrainType.HasValue
            ? _strategusRouting.GetTerrainSpeedMultiplier(currentTerrainType.Value)
            : 1.0;
        double troopInfluence = TroopInfluence(party.Troops);
        double mountInfluence = MountsInfluence(party.Troops, party.Items);
        double baseSpeedWithoutTerrain = baseSpeed * weightFactor * mountInfluence * troopInfluence;

        return new PartySpeed
        {
            BaseSpeed = baseSpeed,
            TerrainSpeedFactor = terrainSpeedFactor,
            CurrentTerrainType = currentTerrainType,
            WeightFactor = weightFactor,
            MountInfluence = mountInfluence,
            TroopInfluence = troopInfluence,
            BaseSpeedWithoutTerrain = baseSpeedWithoutTerrain,
            FinalSpeed = baseSpeedWithoutTerrain * terrainSpeedFactor,
        };
    }

    private static double TroopInfluence(float troops)
    {
        // Troops                  | troopInfluence |
        // ------------------------+----------------+
        //  1                      |        2=2/1   |
        //  100                    |        1=2/2   |
        //  1000                   |          2/3   |
        //  10000                  |          2/4   |
        // this divide the speed of the army by the order of magnitude of its size.
        // 10000 is four zeros so the denominator is 4
        return 2 / (1 + Math.Log10(1 + troops / 10));
    }

    private double MountsInfluence(float troops, List<PartyItem> partyItems)
    {
        int mounts = 0;
        foreach (PartyItem partyItem in partyItems.OrderByDescending(i => i.Item!.Mount!.HitPoints))
        {
            mounts += partyItem.Count;
            int mountSpeed = partyItem.Item!.Mount!.HitPoints / 100;
            if (mounts >= troops && mountSpeed >= forcedMarchSpeed)
            {
                // This is in case there is enough mount for everyone soldier to be mounted. The soldier will choose
                // by default the fastest mounts they can find. In this case the speed of the army is the speed of the
                // slowest mount among the used one (which is worst of the top tier mounts). Currently we're using the
                // hit points to calculate the speed, because strategus is about sustained speed. Marathon runner are
                // more suited for long distance than sprint runners. Manually designed speed for mounts could be added
                // later for more fine tuning.
                return mountSpeed;
            }
        }

        // This is in case there is not enough mounts for every soldier to be mounted the model for this is assuming
        // some of the soldiers have to walk. Since they can change places with someone that is already on a mount,
        // they can afford to walk faster the more the ratio troops / mounts is close to 1 , the more they can afford.
        return forcedMarchSpeed * mounts / troops + (1 - mounts / troops);
    }
}

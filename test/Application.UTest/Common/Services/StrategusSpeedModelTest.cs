using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Terrains;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class StrategusSpeedModelTest
{
    [Test]
    public void TroopsShouldUseTheBestMountTheyHave()
    {
        Party party1 = new()
        {
            Troops = 10,
            Items =
            [
                PartyItemMount(450, 5),
                PartyItemMount(350, 5),
                PartyItemMount(250, 5),
            ],
        };
        Party party2 = new()
        {
            Troops = 10,
            Items =
            [
                PartyItemMount(450, 5),
                PartyItemMount(350, 10),
                PartyItemMount(250, 10),
            ],
        };
        StrategusSpeedModel speedModel = CreateSpeedModel();
        Assert.That(speedModel.ComputePartySpeed(party1, null).FinalSpeed,
            Is.GreaterThanOrEqualTo(speedModel.ComputePartySpeed(party2, null).FinalSpeed));
    }

    [Test]
    public void RecruitingTroopShouldDecreaseSpeed()
    {
        int fastHorseCount = 150;
        int mediumSpeedHorseCount = 100;
        int slowHorseCount = 50;
        int totalHorseCount = fastHorseCount + mediumSpeedHorseCount + slowHorseCount;
        double previousSpeed = double.MaxValue;
        StrategusSpeedModel speedModel = CreateSpeedModel();
        for (int troops = 10; troops <= 1000; troops += 10)
        {
            Party party = new()
            {
                Troops = troops,
                Items =
                [
                    PartyItemMount(450, fastHorseCount),
                    PartyItemMount(350, mediumSpeedHorseCount),
                    PartyItemMount(250, slowHorseCount),
                ],
            };
            double speed = speedModel.ComputePartySpeed(party, null).FinalSpeed;
            if (troops < totalHorseCount)
            {
                /*
                this is in case there is enough mount for everyone soldier to be mounted.
                The soldier will choose by default the fastest mounts they can find
                In this case the speed of the army is the speed of the slowest mount among the used one
                (which is worst of the top tier mounts) .
                In this case the speed may not increase but should not decrease
                */
                Assert.That(speed, Is.LessThanOrEqualTo(previousSpeed));
            }
            else
            {
                /*
                This is in case there is not enough mounts for every soldier to be mounted the model for this is
                assuming some of the soldiers have to walk. The more of them walk , the slowest the party get.
                The speed should strictly decrease.
                */
                Assert.That(speed, Is.LessThan(previousSpeed));
            }

            previousSpeed = speed;
        }
    }

    [Test]
    public void BuyingMountsShouldIncreaseSpeed()
    {
        double previousSpeed = 0;
        StrategusSpeedModel speedModel = CreateSpeedModel();
        for (int mountCountFactor = 1; mountCountFactor <= 100; mountCountFactor++)
        {
            Party party = new()
            {
                Troops = 1000,
                Items =
                [
                    PartyItemMount(450, 6 * mountCountFactor),
                    PartyItemMount(350, 2 * mountCountFactor),
                    PartyItemMount(250, 2 * mountCountFactor),
                ],
            };
            double speed = speedModel.ComputePartySpeed(party, null).FinalSpeed;
            Assert.That(speed, Is.GreaterThan(previousSpeed));
            previousSpeed = speed;
        }
    }

    [Test]
    public void IdenticalTroopsWithDifferentTerrainShouldHaveDifferentSpeed()
    {
        Party party = new()
        {
            Troops = 100,
            Items =
            [
                PartyItemMount(450, 50),
                PartyItemMount(350, 30),
                PartyItemMount(250, 20),
            ],
        };
        StrategusSpeedModel speedModel = CreateSpeedModelWithTerrainMultipliers();
        double plainSpeed = speedModel.ComputePartySpeed(party, TerrainType.Plain).FinalSpeed;
        double thickForestSpeed = speedModel.ComputePartySpeed(party, TerrainType.ThickForest).FinalSpeed;

        Assert.That(plainSpeed, Is.GreaterThan(thickForestSpeed));
    }

    private static StrategusSpeedModel CreateSpeedModel()
    {
        var mockRouting = new Mock<IStrategusRouting>();
        mockRouting.Setup(m => m.GetTerrainSpeedMultiplier(It.IsAny<TerrainType?>()))
            .Returns(1.0);
        return new StrategusSpeedModel(mockRouting.Object);
    }

    private static StrategusSpeedModel CreateSpeedModelWithTerrainMultipliers()
    {
        var mockRouting = new Mock<IStrategusRouting>();
        mockRouting.Setup(m => m.GetTerrainSpeedMultiplier(TerrainType.Plain)).Returns(1.0);
        mockRouting.Setup(m => m.GetTerrainSpeedMultiplier(TerrainType.ThickForest)).Returns(0.5);
        mockRouting.Setup(m => m.GetTerrainSpeedMultiplier(null)).Returns(1.0);
        return new StrategusSpeedModel(mockRouting.Object);
    }

    private static PartyItem PartyItemMount(int hitPoints, int count)
    {
        return new()
        {
            Item = new Item { Mount = new ItemMountComponent { HitPoints = hitPoints } },
            Count = count,
        };
    }
}

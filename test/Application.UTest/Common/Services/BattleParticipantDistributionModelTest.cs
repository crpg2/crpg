using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class BattleParticipantDistributionModelTest
{
    private static readonly BattleParticipantUniformDistributionModel Distribution = new();

    [Test]
    public void ShouldDistributeUniformly()
    {
        BattleFighter[] fighters =
        {
            NewFighter(100, BattleSide.Attacker),
            NewFighter(200, BattleSide.Attacker),
            NewFighter(300, BattleSide.Attacker),
            NewFighter(500, BattleSide.Defender),
            NewFighter(500, BattleSide.Defender),
        };

        int battleSlots = 100;
        Distribution.DistributeParticipants(fighters, battleSlots);

        Assert.That(fighters[0].ParticipantSlots, Is.EqualTo(16));
        Assert.That(fighters[1].ParticipantSlots, Is.EqualTo(32));
        Assert.That(fighters[2].ParticipantSlots, Is.EqualTo(49));
        Assert.That(fighters.Take(3).Sum(f => f.ParticipantSlots), Is.EqualTo(battleSlots - 3));

        Assert.That(fighters[3].ParticipantSlots, Is.EqualTo(49));
        Assert.That(fighters[4].ParticipantSlots, Is.EqualTo(49));
        Assert.That(fighters.Skip(3).Sum(f => f.ParticipantSlots), Is.EqualTo(battleSlots - 2));
    }

    [Test]
    public void ShouldIgnoreDecimalsOfPartyTroops()
    {
        BattleFighter[] fighters =
        {
            NewFighter(2.9f, BattleSide.Attacker),
            NewFighter(2.8f, BattleSide.Attacker),
            NewFighter(2.7f, BattleSide.Attacker),
        };

        Distribution.DistributeParticipants(fighters, 6);
        Assert.That(fighters[0].ParticipantSlots, Is.EqualTo(1));
        Assert.That(fighters[1].ParticipantSlots, Is.EqualTo(1));
        Assert.That(fighters[2].ParticipantSlots, Is.EqualTo(1));
    }

    [Test]
    public void ShouldGiveRemainingOfTheSlotsDivisionToTheFirstFighters()
    {
        BattleFighter[] fighters =
        {
            NewFighter(2f, BattleSide.Attacker),
            NewFighter(2f, BattleSide.Attacker),
            NewFighter(2f, BattleSide.Attacker),
        };

        Distribution.DistributeParticipants(fighters, 8);
        Assert.That(fighters[0].ParticipantSlots, Is.EqualTo(2));
        Assert.That(fighters[1].ParticipantSlots, Is.EqualTo(2));
        Assert.That(fighters[2].ParticipantSlots, Is.EqualTo(1));
    }

    private static BattleFighter NewFighter(float troops, BattleSide side) => new()
    {
        Party = new Party { Troops = troops, User = new User() },
        Side = side,
    };
}

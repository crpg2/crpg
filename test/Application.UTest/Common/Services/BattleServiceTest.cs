using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class BattleServiceTest : TestBase
{
    private readonly BattleService _battleService = new();

    // TODO: FIXME:
    // [Test]
    // public void MapToBattleDetailedViewModel_ShouldMapCorrectly()
    // {
    //     var attackerUser = new User();
    //     var defenderUser = new User();

    // var attackerParty = new Party { User = attackerUser, Troops = 20 };
    //     var defenderParty = new Party { User = defenderUser, Troops = 15 };

    // var attackerCommander = new BattleFighter
    //     {
    //         Side = BattleSide.Attacker,
    //         Commander = true,
    //         Party = attackerParty,
    //     };

    // var defenderCommander = new BattleFighter
    //     {
    //         Side = BattleSide.Defender,
    //         Commander = true,
    //         Party = defenderParty,
    //     };

    // var otherFighter = new BattleFighter
    //     {
    //         Side = BattleSide.Attacker,
    //         Commander = false,
    //         Party = new Party { Troops = 10 },
    //     };

    // var battle = new Battle
    //     {
    //         Id = 1,
    //         Phase = BattlePhase.Scheduled,
    //         Fighters = new List<BattleFighter> { attackerCommander, defenderCommander, otherFighter },
    //         CreatedAt = DateTime.UtcNow,
    //         ScheduledFor = DateTime.UtcNow.AddHours(2),
    //     };

    // var vm = _battleService.MapToBattleDetailedViewModel(Mapper, battle);

    // Assert.That(vm.Id, Is.EqualTo(1));
    //     Assert.That(vm.AttackerTotalTroops, Is.EqualTo(30)); // 20 + 10
    //     Assert.That(vm.DefenderTotalTroops, Is.EqualTo(15));
    //     Assert.That(vm.Attacker.Commander, Is.True);
    //     Assert.That(vm.Defender?.Commander, Is.True);
    //     Assert.That(vm.Type, Is.EqualTo(BattleType.Battle));
    // }
}

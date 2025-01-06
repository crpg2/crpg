using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class RemoveBattleMercenaryCommandTest : TestBase
{
    private IBattleService BattleService { get; } = new BattleService();
    {
        [Test]
        public async Task ShouldReturnErrorIfUserNotFound()
        {
            RemoveBattleMercenaryCommand.Handler handler = new(ActDb, BattleService);
            var res = await handler.Handle(new RemoveBattleMercenaryCommand
            {
                UserId = 99,
                BattleId = 99,
            }, CancellationToken.None);

            Assert.That(res.Errors, Is.Not.Null);
            Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
        }
    }
}

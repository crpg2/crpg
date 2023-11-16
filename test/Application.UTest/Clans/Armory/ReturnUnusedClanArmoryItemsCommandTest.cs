﻿using Crpg.Application.Clans.Commands.Armory;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans.Armory;
public class ReturnUnusedClanArmoryItemsCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturn()
    {
        await ClanArmoryTestHelper.CommonSetUp(ArrangeDb);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user0", 2);
        await ClanArmoryTestHelper.AddItems(ArrangeDb, "user1", 2);
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user2", 2);
        await ClanArmoryTestHelper.BorrowItems(ArrangeDb, "user3", 2);
        await ArrangeDb.Users.ForEachAsync(u => u.UpdatedAt = DateTime.UtcNow.Subtract(TimeSpan.FromDays(10)));
        await ArrangeDb.SaveChangesAsync();

        Assert.That(ActDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(4));

        var handler = new ReturnUnusedClanArmoryItemsCommand.Handler(ActDb);
        var result = await handler.Handle(new ReturnUnusedClanArmoryItemsCommand
        {
            Timeout = TimeSpan.FromDays(3),
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);

        Assert.That(AssertDb.ClanArmoryBorrowedItems.Count(), Is.EqualTo(0));
    }
}

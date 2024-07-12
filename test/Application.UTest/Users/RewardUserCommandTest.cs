﻿using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class RewardUserCommandTest : TestBase
{
    [Test]
    public async Task UserNotFound()
    {
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        RewardUserCommand.Handler handler = new(activityLogServiceMock.Object, ActDb, Mapper);
        var res = await handler.Handle(new RewardUserCommand
        {
            UserId = 1,
            Gold = 100,
            HeirloomPoints = 2,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task PersonalItemNotFound()
    {
        User user = new()
        {
            Gold = 200,
            HeirloomPoints = 1,
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        RewardUserCommand.Handler handler = new(activityLogServiceMock.Object, ActDb, Mapper);
        var res = await handler.Handle(new RewardUserCommand
        {
            UserId = 1,
            Gold = 100,
            HeirloomPoints = 2,
            ItemId = "crpg_personal_item_1",
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotFound));
    }

    [Test]
    public async Task PersonalItemAlreadyExist()
    {
        ArrangeDb.Users.Add(new() { });
        ArrangeDb.Items.Add(new() { Id = "crpg_personal_item_1" });
        ArrangeDb.PersonalItems.Add(new() { ItemId = "crpg_personal_item_1", UserId = 1 });

        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        RewardUserCommand.Handler handler = new(activityLogServiceMock.Object, ActDb, Mapper);
        var res = await handler.Handle(new RewardUserCommand
        {
            UserId = 1,
            Gold = 100,
            HeirloomPoints = 2,
            ItemId = "crpg_personal_item_1",
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PersonalItemAlreadyExist));
    }

    [Test]
    public async Task Basic()
    {
        User user = new()
        {
            Gold = 200,
            HeirloomPoints = 1,
        };
        ArrangeDb.Users.Add(user);

        Item item1 = new() { Id = "crpg_personal_item_1" };
        ArrangeDb.Items.Add(item1);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        RewardUserCommand.Handler handler = new(activityLogServiceMock.Object, ActDb, Mapper);
        var res = await handler.Handle(new RewardUserCommand
        {
            UserId = user.Id,
            Gold = 100,
            HeirloomPoints = 2,
            ItemId = "crpg_personal_item_1",
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Gold, Is.EqualTo(300));
        Assert.That(res.Data.HeirloomPoints, Is.EqualTo(3));

        var userDb = await AssertDb.Users
            .FirstAsync(u => u.Id == user.Id);
        Assert.That(userDb.PersonalItems.Count, Is.EqualTo(1));
    }
}

using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Commands;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class RewardUserCommandTest : TestBase
{
    private static readonly Mock<IActivityLogService> ActivityLogService = new() { DefaultValue = DefaultValue.Mock };
    private static readonly Mock<IUserNotificationService> UserNotificationService = new() { DefaultValue = DefaultValue.Mock };

    [Test]
    public async Task UserNotFound()
    {
        RewardUserCommand.Handler handler = new(ActDb, Mapper, ActivityLogService.Object, UserNotificationService.Object);
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

        RewardUserCommand.Handler handler = new(ActDb, Mapper, ActivityLogService.Object, UserNotificationService.Object);
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
        ArrangeDb.Users.Add(new() { Items = { new() { Item = new() { Id = "crpg_personal_item_1" }, PersonalItem = new() } } });
        await ArrangeDb.SaveChangesAsync();

        RewardUserCommand.Handler handler = new(ActDb, Mapper, ActivityLogService.Object, UserNotificationService.Object);
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
        ArrangeDb.Items.Add(new() { Id = "crpg_personal_item_1" });
        await ArrangeDb.SaveChangesAsync();

        RewardUserCommand.Handler handler = new(ActDb, Mapper, ActivityLogService.Object, UserNotificationService.Object);
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
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);
        Assert.That(userDb.Items.Count, Is.EqualTo(1));
    }
}

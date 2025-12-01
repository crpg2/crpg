using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Queries;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class GetUserQueryTest : TestBase
{
    [Test]
    public async Task TestWhenUserDoesntExist()
    {
        Mock<IUserService> userServiceMock = new();
        GetUserQuery.Handler handler = new(ActDb, Mapper, userServiceMock.Object);
        var result = await handler.Handle(new GetUserQuery
        {
            UserId = 1,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task TestWhenUserExists()
    {
        Mock<IUserService> userServiceMock = new();
        User dbUser = new()
        {
            PlatformUserId = "13948192759205810",
            Name = "def",
            Role = Role.Admin,
            Avatar = new Uri("http://ghi.klm"),
        };
        ArrangeDb.Users.Add(dbUser);
        await ArrangeDb.SaveChangesAsync();

        GetUserQuery.Handler handler = new(ActDb, Mapper, userServiceMock.Object);
        var user = await handler.Handle(new GetUserQuery
        {
            UserId = dbUser.Id,
        }, CancellationToken.None);

        Assert.That(user, Is.Not.Null);
    }
}

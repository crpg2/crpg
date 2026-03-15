using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Users.Queries;
using Crpg.Application.UTest.Marketplace;
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

    [Test]
    public async Task TestReservedGoldAndHeirloomPoints()
    {
        Mock<IUserService> userServiceMock = new();
        User dbUser = new();
        dbUser.Offers.Add(MarketplaceOfferFactory.CreateOffer(sellerId: 0, goldFee: 10, offeredGold: 100, offeredHeirloomPoints: 5, requestedGold: 200));
        dbUser.Offers.Add(MarketplaceOfferFactory.CreateOffer(sellerId: 0, goldFee: 5, offeredGold: 50, offeredHeirloomPoints: 3, requestedGold: 300));
        ArrangeDb.Users.Add(dbUser);
        await ArrangeDb.SaveChangesAsync();

        GetUserQuery.Handler handler = new(ActDb, Mapper, userServiceMock.Object);
        var result = await handler.Handle(new GetUserQuery { UserId = dbUser.Id }, CancellationToken.None);

        Assert.That(result.Data!.ReservedGold, Is.EqualTo(165)); // 100 + 10 + 50 + 5
        Assert.That(result.Data!.ReservedHeirloomPoints, Is.EqualTo(8));
    }

    [Test]
    public async Task TestActiveOffersCount()
    {
        Mock<IUserService> userServiceMock = new();
        User dbUser = new();
        dbUser.Offers.Add(MarketplaceOfferFactory.CreateOffer(sellerId: 0, offeredGold: 10, requestedGold: 20));
        dbUser.Offers.Add(MarketplaceOfferFactory.CreateOffer(sellerId: 0, offeredGold: 30, requestedGold: 40));
        dbUser.Offers.Add(MarketplaceOfferFactory.CreateOffer(sellerId: 0, offeredGold: 50, requestedGold: 60));
        ArrangeDb.Users.Add(dbUser);
        await ArrangeDb.SaveChangesAsync();

        GetUserQuery.Handler handler = new(ActDb, Mapper, userServiceMock.Object);
        var result = await handler.Handle(new GetUserQuery { UserId = dbUser.Id }, CancellationToken.None);

        Assert.That(result.Data!.ActiveMarketplaceOffersCount, Is.EqualTo(3));
    }
}

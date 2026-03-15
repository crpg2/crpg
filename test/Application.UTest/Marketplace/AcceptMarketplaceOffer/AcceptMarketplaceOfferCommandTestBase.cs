using Crpg.Application.Common.Services;
using Crpg.Application.Marketplace.Commands;
using Crpg.Application.Marketplace.Services;
using Crpg.Sdk.Abstractions;
using Moq;

namespace Crpg.Application.UTest.Marketplace.AcceptMarketplaceOffer;

internal abstract class AcceptMarketplaceOfferCommandTestBase : TestBase
{
    protected AcceptMarketplaceOfferCommand.Handler CreateHandler(
        IActivityLogService? activityLog = null,
        IUserNotificationService? notifications = null,
        IDateTime? dateTime = null,
        IMarketplaceService? marketplace = null) =>
        new(ActDb,
            activityLog ?? new ActivityLogService(new MetadataService()),
            notifications ?? new UserNotificationService(new MetadataService()),
            dateTime ?? Mock.Of<IDateTime>(),
            marketplace ?? Mock.Of<IMarketplaceService>());
}

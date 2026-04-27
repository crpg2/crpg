using Crpg.Application.Quests.Queries;
using Crpg.Application.Quests.Services;
using Crpg.Domain.Entities.Quests;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Quests;

public class GetUserQuestsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnEmptyListIfNoQuests()
    {
        var user = new User();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IQuestEvaluationService> questEvaluationServiceMock = new();
        var handler = new GetUserQuestsQuery.Handler(ActDb, Mapper, questEvaluationServiceMock.Object);

        var result = await handler.Handle(new GetUserQuestsQuery
        {
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        Assert.That(result.Data, Is.Empty);
    }

    [Test]
    public async Task ShouldReturnQuestsWithCurrentValues()
    {
        var user = new User();
        var questDefinition1 = new QuestDefinition
        {
            Id = 1,
            RequiredValue = 10,
            RewardGold = 100,
            RewardExperience = 200,
        };
        var questDefinition2 = new QuestDefinition
        {
            Id = 2,
            RequiredValue = 20,
            RewardGold = 200,
            RewardExperience = 400,
        };
        var userQuest1 = new UserQuest
        {
            Id = 10,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = questDefinition1.Id,
            QuestDefinition = questDefinition1,
            IsRewardClaimed = false,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
        };
        var userQuest2 = new UserQuest
        {
            Id = 11,
            UserId = user.Id,
            User = user,
            QuestDefinitionId = questDefinition2.Id,
            QuestDefinition = questDefinition2,
            IsRewardClaimed = true,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.QuestDefinitions.AddRange(questDefinition1, questDefinition2);
        ArrangeDb.UserQuests.AddRange(userQuest1, userQuest2);
        await ArrangeDb.SaveChangesAsync();

        Mock<IQuestEvaluationService> questEvaluationServiceMock = new();
        questEvaluationServiceMock.Setup(q => q.ComputeCurrentValuesAsync(It.IsAny<List<UserQuest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<UserQuest> uqs, CancellationToken ct) =>
                uqs.ToDictionary(uq => uq.Id, uq => uq.Id == userQuest1.Id ? 8 : 25));

        var handler = new GetUserQuestsQuery.Handler(ActDb, Mapper, questEvaluationServiceMock.Object);

        var result = await handler.Handle(new GetUserQuestsQuery
        {
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        var data = result.Data!;
        Assert.That(data, Has.Count.EqualTo(2));

        var vm1 = data.FirstOrDefault(vm => vm.Id == userQuest1.Id);
        Assert.That(vm1, Is.Not.Null);
        Assert.That(vm1!.CurrentValue, Is.EqualTo(8)); // not capped because less than required

        var vm2 = data.FirstOrDefault(vm => vm.Id == userQuest2.Id);
        Assert.That(vm2, Is.Not.Null);
        Assert.That(vm2!.CurrentValue, Is.EqualTo(20)); // capped at required value
        Assert.That(vm2.IsRewardClaimed, Is.True);
    }

    [Test]
    public async Task ShouldNotReturnQuestsForOtherUsers()
    {
        var user1 = new User();
        var user2 = new User();
        var questDefinition = new QuestDefinition
        {
            Id = 1,
            RequiredValue = 10,
        };
        var userQuest1 = new UserQuest
        {
            Id = 10,
            UserId = user1.Id,
            User = user1,
            QuestDefinitionId = questDefinition.Id,
            QuestDefinition = questDefinition,
        };
        var userQuest2 = new UserQuest
        {
            Id = 11,
            UserId = user2.Id,
            User = user2,
            QuestDefinitionId = questDefinition.Id,
            QuestDefinition = questDefinition,
        };
        ArrangeDb.Users.AddRange(user1, user2);
        ArrangeDb.QuestDefinitions.Add(questDefinition);
        ArrangeDb.UserQuests.AddRange(userQuest1, userQuest2);
        await ArrangeDb.SaveChangesAsync();

        Mock<IQuestEvaluationService> questEvaluationServiceMock = new();
        questEvaluationServiceMock.Setup(q => q.ComputeCurrentValuesAsync(It.IsAny<List<UserQuest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<UserQuest> uqs, CancellationToken ct) =>
                uqs.ToDictionary(uq => uq.Id, uq => uq.Id == userQuest1.Id ? 5 : 15));

        var handler = new GetUserQuestsQuery.Handler(ActDb, Mapper, questEvaluationServiceMock.Object);

        var result = await handler.Handle(new GetUserQuestsQuery
        {
            UserId = user1.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Null);
        var data = result.Data!;
        Assert.That(data, Has.Count.EqualTo(1));
        Assert.That(data[0].Id, Is.EqualTo(userQuest1.Id));
        Assert.That(data[0].CurrentValue, Is.EqualTo(5));
    }
}

using System.Text.Json.Serialization;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Quests.Services;
using Crpg.Domain.Entities.Quests;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Quests.Commands;

public record RerollQuestCommand : IMediatorRequest
{
    [JsonIgnore]
    public int UserId { get; init; }

    [JsonIgnore]
    public int UserQuestId { get; init; }

    internal class Handler(
        ICrpgDbContext db,
        IDateTime dateTime,
        IActivityLogService activityLogService,
        IQuestAssignmentService questAssignmentService,
        Constants constants)
        : IMediatorRequestHandler<RerollQuestCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RerollQuestCommand>();

        public async ValueTask<Result> Handle(RerollQuestCommand req,
            CancellationToken cancellationToken)
        {
            int price = constants.QuestRerollDailyQuestPrice;
            var userQuest = await db.UserQuests
                .Include(uq => uq.QuestDefinition)
                .Include(uq => uq.User!)
                .FirstOrDefaultAsync(
                    uq => uq.Id == req.UserQuestId && uq.UserId == req.UserId &&
                          uq.QuestDefinition!.Type == QuestType.Daily, cancellationToken);

            if (userQuest == null)
            {
                return new(CommonErrors.UserQuestNotFound(req.UserQuestId, req.UserId));
            }

            if (userQuest.IsRewardClaimed)
            {
                return new(CommonErrors.QuestRewardAlreadyClaimed(req.UserQuestId));
            }

            if (userQuest.ExpiresAt <= dateTime.UtcNow)
            {
                return new(CommonErrors.QuestExpired(req.UserQuestId));
            }

            if (userQuest.QuestDefinition == null)
            {
                return new(CommonErrors.QuestDefinitionNotFound(userQuest.QuestDefinitionId));
            }

            var user = userQuest.User!;
            if (user.Gold < price)
            {
                return new(CommonErrors.NotEnoughGold(price, user.Gold));
            }

            user.Gold -= price;

            var newUserQuest = await questAssignmentService.ReplaceDailyUserQuestAsync(userQuest, cancellationToken);

            db.ActivityLogs.Add(activityLogService.CreateQuestRerolledLog(
                req.UserId, userQuest.Id, newUserQuest.Id, price));

            await db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' rerolled quest '{1}' to new quest '{2}' for {3} gold",
                req.UserId, userQuest.Id, newUserQuest.Id, price);

            return Result.NoErrors;
        }
    }
}

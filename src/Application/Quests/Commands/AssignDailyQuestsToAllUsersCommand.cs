using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Quests.Services;

namespace Crpg.Application.Quests.Commands;

public record AssignDailyQuestsToAllUsersCommand : IMediatorRequest
{
    internal class Handler(IQuestAssignmentService questAssignmentService)
        : IMediatorRequestHandler<AssignDailyQuestsToAllUsersCommand>
    {
        public async ValueTask<Result> Handle(AssignDailyQuestsToAllUsersCommand req, CancellationToken cancellationToken)
        {
            await questAssignmentService.AssignDailyQuestsToAllUsersAsync(cancellationToken);
            return Result.NoErrors;
        }
    }
}

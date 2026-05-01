using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Quests.Services;

namespace Crpg.Application.Quests.Commands;

public record AssignWeeklyQuestsToAllUsersCommand : IMediatorRequest
{
    internal class Handler(IQuestAssignmentService questAssignmentService)
        : IMediatorRequestHandler<AssignWeeklyQuestsToAllUsersCommand>
    {
        public async ValueTask<Result> Handle(AssignWeeklyQuestsToAllUsersCommand req,
            CancellationToken cancellationToken)
        {
            await questAssignmentService.AssignWeeklyQuestsToAllUsersAsync(cancellationToken);
            return Result.NoErrors;
        }
    }
}

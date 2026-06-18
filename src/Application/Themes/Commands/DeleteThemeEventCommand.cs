using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Themes.Commands;

public class DeleteThemeEventCommand : IMediatorRequest
{
    public int Id { get; init; }

    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<DeleteThemeEventCommand>
    {
        public async ValueTask<Result> Handle(DeleteThemeEventCommand req, CancellationToken cancellationToken)
        {
            var themeEvent = await db.ThemeEvents.SingleOrDefaultAsync(t => t.Id == req.Id, cancellationToken);
            if (themeEvent == null)
            {
                // If someone tried to remove a theme event that does not exist, might as well just show a thumbs up instead of throwing errors everywhere.
                return new();
            }

            db.ThemeEvents.Remove(themeEvent);
            await db.SaveChangesAsync(cancellationToken);

            return new Result();
        }
    }
}

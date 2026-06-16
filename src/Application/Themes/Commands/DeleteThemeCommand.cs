using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Themes.Commands;

public record DeleteThemeCommand : IMediatorRequest<ThemeViewModel>
{
    public int Id { get; init; }

    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<DeleteThemeCommand>
    {
        public async ValueTask<Result> Handle(DeleteThemeCommand req, CancellationToken cancellationToken)
        {
            var theme = await db.Themes.SingleOrDefaultAsync(t => t.Id == req.Id, cancellationToken);
            if (theme == null)
            {
                // If someone tried to remove a theme that does not exist, might as well just show a thumbs up instead of throwing errors everywhere.
                return new();
            }

            await db.ThemeEvents.RemoveRangeAsync(e => e.EventTheme.Id == req.Id, cancellationToken);

            var taggedItems = await db.Items
                .Include(i => i.Themes)
                .Where(i => i.Themes.Any(t => t.Id == req.Id))
                .ToListAsync(cancellationToken);

            foreach (var item in taggedItems)
            {
                item.Themes.Clear();
            }

            db.Themes.Remove(theme);

            await db.SaveChangesAsync(cancellationToken);

            return new();
        }
    }
}

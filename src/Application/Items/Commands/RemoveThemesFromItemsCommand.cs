using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands;

/// <summary>
/// Removes a set of themes from several items at once. Any other themes on those items are preserved; themes
/// that aren't present are ignored.
/// </summary>
public record RemoveThemesFromItemsCommand : IMediatorRequest
{
    public IList<string> ItemIds { get; init; } = new List<string>();
    public IList<int> ThemeIds { get; init; } = new List<int>();

    public class Validator : AbstractValidator<RemoveThemesFromItemsCommand>
    {
        public Validator()
        {
            RuleFor(r => r.ItemIds).NotEmpty();
            RuleFor(r => r.ThemeIds).NotEmpty();
        }
    }

    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<RemoveThemesFromItemsCommand>
    {
        public async ValueTask<Result> Handle(RemoveThemesFromItemsCommand req, CancellationToken cancellationToken)
        {
            var itemIds = req.ItemIds.Distinct().ToList();
            var items = await db.Items
                .Include(i => i.Themes)
                .Where(i => itemIds.Contains(i.Id))
                .ToListAsync(cancellationToken);
            if (items.Count != itemIds.Count)
            {
                string missingItemId = itemIds.First(id => items.All(i => i.Id != id));
                return new(CommonErrors.ItemNotFound(missingItemId));
            }

            var themeIds = req.ThemeIds.Distinct().ToList();
            var themes = await db.Themes.Where(t => themeIds.Contains(t.Id)).ToListAsync(cancellationToken);
            if (themes.Count != themeIds.Count)
            {
                int missingThemeId = themeIds.First(id => themes.All(t => t.Id != id));
                return new(CommonErrors.ThemeNotFound(missingThemeId));
            }

            foreach (var item in items)
            {
                item.Themes.RemoveAll(t => themeIds.Contains(t.Id));
            }

            await db.SaveChangesAsync(cancellationToken);

            return Result.NoErrors;
        }
    }
}

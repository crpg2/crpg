using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands;

/// <summary>
/// Removes a set of themes from several item families at once. The themes are removed from every rank variant
/// sharing one of the given <see cref="Crpg.Domain.Entities.Items.Item.BaseId"/>s, so upgrades stay in sync. Any
/// other themes on those items are preserved; themes that aren't present are ignored.
/// </summary>
public record RemoveThemesFromItemsCommand : IMediatorRequest
{
    public IList<string> BaseIds { get; init; } = new List<string>();
    public IList<int> ThemeIds { get; init; } = new List<int>();

    public class Validator : AbstractValidator<RemoveThemesFromItemsCommand>
    {
        public Validator()
        {
            RuleFor(r => r.BaseIds).NotEmpty();
            RuleFor(r => r.ThemeIds).NotEmpty();
        }
    }

    internal class Handler(ICrpgDbContext db) : IMediatorRequestHandler<RemoveThemesFromItemsCommand>
    {
        public async ValueTask<Result> Handle(RemoveThemesFromItemsCommand req, CancellationToken cancellationToken)
        {
            var baseIds = req.BaseIds.Distinct().ToList();
            var items = await db.Items
                .Include(i => i.Themes)
                .Where(i => baseIds.Contains(i.BaseId))
                .ToListAsync(cancellationToken);

            var foundBaseIds = items.Select(i => i.BaseId).ToHashSet();
            string? missingBaseId = baseIds.FirstOrDefault(id => !foundBaseIds.Contains(id));
            if (missingBaseId != null)
            {
                return new(CommonErrors.ItemNotFound(missingBaseId));
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

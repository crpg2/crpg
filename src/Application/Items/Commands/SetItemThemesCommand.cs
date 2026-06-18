using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands;

/// <summary>
/// Replaces the set of themes assigned to a single item.
/// </summary>
public record SetItemThemesCommand : IMediatorRequest<ItemViewModel>
{
    [JsonIgnore]
    public string ItemId { get; init; } = string.Empty;
    public IList<int> ThemeIds { get; init; } = new List<int>();

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<SetItemThemesCommand, ItemViewModel>
    {
        public async ValueTask<Result<ItemViewModel>> Handle(SetItemThemesCommand req, CancellationToken cancellationToken)
        {
            var item = await db.Items
                .Include(i => i.Themes)
                .FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
            if (item == null)
            {
                return new(CommonErrors.ItemNotFound(req.ItemId));
            }

            var themeIds = req.ThemeIds.Distinct().ToList();
            var themes = await db.Themes.Where(t => themeIds.Contains(t.Id)).ToListAsync(cancellationToken);
            if (themes.Count != themeIds.Count)
            {
                int missingThemeId = themeIds.First(id => themes.All(t => t.Id != id));
                return new(CommonErrors.ThemeNotFound(missingThemeId));
            }

            item.Themes.Clear();
            item.Themes.AddRange(themes);

            await db.SaveChangesAsync(cancellationToken);

            return new(mapper.Map<ItemViewModel>(item));
        }
    }
}

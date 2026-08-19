using System.Text.Json.Serialization;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Commands;

/// <summary>
/// Replaces the set of themes assigned to an item family. The themes are applied to every rank variant sharing the
/// given <see cref="Crpg.Domain.Entities.Items.Item.BaseId"/>, so upgrades stay in sync.
/// </summary>
public record SetItemThemesCommand : IMediatorRequest<ItemViewModel>
{
    [JsonIgnore]
    public string BaseId { get; init; } = string.Empty;
    public IList<int> ThemeIds { get; init; } = new List<int>();

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<SetItemThemesCommand, ItemViewModel>
    {
        public async ValueTask<Result<ItemViewModel>> Handle(SetItemThemesCommand req, CancellationToken cancellationToken)
        {
            var items = await db.Items
                .Include(i => i.Themes)
                .Where(i => i.BaseId == req.BaseId)
                .ToListAsync(cancellationToken);
            if (items.Count == 0)
            {
                return new(CommonErrors.ItemNotFound(req.BaseId));
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
                item.Themes.Clear();
                item.Themes.AddRange(themes);
            }

            await db.SaveChangesAsync(cancellationToken);

            var representative = items.OrderBy(i => i.Rank).First();
            return new(mapper.Map<ItemViewModel>(representative));
        }
    }
}

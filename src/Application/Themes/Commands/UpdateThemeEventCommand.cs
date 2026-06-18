using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Models;
using Crpg.Domain.Entities.Themes;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Themes.Commands;

public class UpdateThemeEventCommand : IMediatorRequest<ThemeEventViewModel>
{
    public int Id { get; init; }
    public string Name { get; set; } = default!;
    public float GoldMultiplier { get; set; } = 1.0f;
    public float ExpMultiplier { get; set; } = 1.0f;
    public DateTimeOffset ActiveFromUtc { get; set; }
    public DateTimeOffset? ActiveUntilUtc { get; set; }
    public List<ThemeEquipmentSlot> RequiredEquipmentSlotsMatchingTheme { get; set; } = new();

    /// <summary>
    /// Minimum number of themed items the player must equip. When null, it defaults to the number of required slots.
    /// </summary>
    public int? MinimumThemedItemsEquipped { get; set; }
    public int ThemeId { get; set; }

    public class Validator : AbstractValidator<UpdateThemeEventCommand>
    {
        public Validator()
        {
            RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Theme event name is required.")
                .MaximumLength(100).WithMessage("Theme event name cannot exceed 100 characters.");

            RuleFor(t => t.GoldMultiplier).GreaterThan(0);
            RuleFor(t => t.ExpMultiplier).GreaterThan(0);

            RuleFor(t => t.ActiveFromUtc)
                .NotEmpty()
                .Must(dt => dt.Offset == TimeSpan.Zero)
                .WithMessage("ActiveFromUTC must be UTC.");

            RuleFor(t => t.ActiveUntilUtc)
                .Must(dt => dt == null || dt.Value.Offset == TimeSpan.Zero)
                .WithMessage("ActiveUntilUTC must be UTC.");
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<UpdateThemeEventCommand, ThemeEventViewModel>
    {
        public async ValueTask<Result<ThemeEventViewModel>> Handle(UpdateThemeEventCommand req, CancellationToken cancellationToken)
        {
            var themeEvent = await db.ThemeEvents
                .Include(e => e.EventTheme)
                .SingleOrDefaultAsync(t => t.Id == req.Id, cancellationToken);

            if (themeEvent == null)
            {
                return new(CommonErrors.ThemeEventNotFound(req.Id));
            }

            if (themeEvent.EventTheme.Id != req.ThemeId)
            {
                var theme = await db.Themes.FindAsync(req.ThemeId, cancellationToken);
                if (theme == null)
                {
                    return new(CommonErrors.ThemeNotFound(req.ThemeId));
                }

                themeEvent.EventTheme = theme;
            }

            themeEvent.Name = req.Name;
            themeEvent.GoldMultiplier = req.GoldMultiplier;
            themeEvent.ExpMultiplier = req.ExpMultiplier;
            themeEvent.ActiveFromUtc = req.ActiveFromUtc;
            themeEvent.ActiveUntilUtc = req.ActiveUntilUtc;
            themeEvent.RequiredEquipmentSlotsMatchingTheme = req.RequiredEquipmentSlotsMatchingTheme;
            themeEvent.MinimumThemedItemsEquipped = req.MinimumThemedItemsEquipped ?? req.RequiredEquipmentSlotsMatchingTheme.Count;

            await db.SaveChangesAsync(cancellationToken);

            return new(mapper.Map<ThemeEventViewModel>(themeEvent));
        }
    }
}

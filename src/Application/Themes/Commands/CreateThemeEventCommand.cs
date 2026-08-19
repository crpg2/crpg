using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Models;
using Crpg.Domain.Entities.Themes;
using FluentValidation;

namespace Crpg.Application.Themes.Commands;

public record CreateThemeEventCommand : IMediatorRequest<ThemeEventViewModel>
{

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

    public int ThemeId { get; set; } = default!;

    public class Validator : AbstractValidator<CreateThemeEventCommand>
    {
        public Validator()
        {
            RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Theme name is required.")
                .MaximumLength(100).WithMessage("Theme name cannot exceed 100 characters.");

            RuleFor(r => r.GoldMultiplier).GreaterThanOrEqualTo(1);
            RuleFor(r => r.ExpMultiplier).GreaterThanOrEqualTo(1);

            RuleFor(r => r.ActiveFromUtc)
                .NotEmpty()
                .Must(dt => dt.Offset == TimeSpan.Zero)
                .WithMessage("ActiveFromUtc must be UTC.");

            RuleFor(r => r.ActiveUntilUtc)
                .Must(dt => dt == null || dt.Value.Offset == TimeSpan.Zero)
                .WithMessage("ActiveUntilUtc must be UTC.")
                .Must((cmd, until) => until == null || until > cmd.ActiveFromUtc)
                .WithMessage("ActiveUntilUtc must be after ActiveFromUtc.");
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<CreateThemeEventCommand, ThemeEventViewModel>
    {
        public async ValueTask<Result<ThemeEventViewModel>> Handle(CreateThemeEventCommand req, CancellationToken cancellationToken)
        {
            var theme = await db.Themes.FindAsync([req.ThemeId], cancellationToken: cancellationToken);
            if (theme is null)
            {
                return new(CommonErrors.ThemeNotFound(req.ThemeId));
            }

            var themeEventToAdd = new ThemeEvent(
                    name: req.Name,
                    goldMultiplier: req.GoldMultiplier,
                    expMultiplier: req.ExpMultiplier,
                    activeFromUtc: req.ActiveFromUtc,
                    activeUntilUtc: req.ActiveUntilUtc,
                    requiredEquipmentSlotsMatchingTheme: req.RequiredEquipmentSlotsMatchingTheme,
                    minimumThemedItemsEquipped: req.MinimumThemedItemsEquipped ?? req.RequiredEquipmentSlotsMatchingTheme.Count,
                    theme: theme);

            await db.ThemeEvents.AddAsync(themeEventToAdd);
            await db.SaveChangesAsync(cancellationToken);

            return new(mapper.Map<ThemeEventViewModel>(themeEventToAdd));
        }
    }
}

using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Models;
using Crpg.Domain.Entities.Themes;
using FluentValidation;

namespace Crpg.Application.Themes.Commands;

public record CreateThemeCommand : IMediatorRequest<ThemeViewModel>
{
    public string Name { get; set; } = default!;

    public class Validator : AbstractValidator<CreateThemeCommand>
    {
        public Validator()
        {
            RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Theme name is required.")
                .MaximumLength(100).WithMessage("Theme name cannot exceed 100 characters.");
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<CreateThemeCommand, ThemeViewModel>
    {
        public async ValueTask<Result<ThemeViewModel>> Handle(CreateThemeCommand req, CancellationToken cancellationToken)
        {
            var themeToAdd = new Theme(req.Name);

            await db.Themes.AddAsync(themeToAdd);
            await db.SaveChangesAsync(cancellationToken);

            return new(mapper.Map<ThemeViewModel>(themeToAdd));
        }
    }
}

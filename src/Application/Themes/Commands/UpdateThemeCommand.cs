using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Themes.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Themes.Commands;

public record UpdateThemeCommand : IMediatorRequest<ThemeViewModel>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public class Validator : AbstractValidator<UpdateThemeCommand>
    {
        public Validator()
        {
            RuleFor(t => t.Name)
                .NotEmpty().WithMessage("Theme name is required.")
                .MaximumLength(100).WithMessage("Theme name cannot exceed 100 characters.");
        }
    }

    internal class Handler(ICrpgDbContext db, IMapper mapper) : IMediatorRequestHandler<UpdateThemeCommand, ThemeViewModel>
    {
        public async ValueTask<Result<ThemeViewModel>> Handle(UpdateThemeCommand req, CancellationToken cancellationToken)
        {
            var theme = await db.Themes.SingleOrDefaultAsync(t => t.Id == req.Id, cancellationToken);
            if (theme == null)
            {
                return new(CommonErrors.ThemeNotFound(req.Id));
            }

            theme.Name = req.Name;

            await db.SaveChangesAsync(cancellationToken);

            return new(mapper.Map<ThemeViewModel>(theme));
        }
    }
}

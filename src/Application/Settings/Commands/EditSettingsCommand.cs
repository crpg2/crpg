using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Settings.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Settings.Commands;

public record EditSettingsCommand : IMediatorRequest<SettingsViewModel>
{
    public string? Discord { get; set; }
    public string? Steam { get; set; }
    public string? Patreon { get; set; }
    public string? Github { get; set; }
    public string? Reddit { get; set; }
    public string? ModDb { get; set; }
    public string? HappyHours { get; set; }

    public class Validator : AbstractValidator<EditSettingsCommand>
    {
        public Validator()
        {
            RuleFor(x => x.HappyHours)
                .Must((d, c) => BeValidHappyHoursFormat(c))
                .When(x => !string.IsNullOrWhiteSpace(x.HappyHours))
                .WithMessage("HappyHours must be in the format: Region|HH:mm|HH:mm|TimeZone,...");
        }

        private bool BeValidHappyHoursFormat(string? happyHours)
        {
            if (string.IsNullOrWhiteSpace(happyHours))
            {
                return true;
            }

            string[] entries = happyHours.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (string entry in entries)
            {
                string[] parts = entry.Split('|');
                if (parts.Length != 4)
                {
                    return false;
                }

                // Validate region
                if (string.IsNullOrWhiteSpace(parts[0]))
                {
                    return false;
                }

                // Validate time format
                if (!TimeSpan.TryParse(parts[1], out _) || !TimeSpan.TryParse(parts[2], out _))
                {
                    return false;
                }

                // Validate timezone (basic check)
                if (string.IsNullOrWhiteSpace(parts[3]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal class Handler : IMediatorRequestHandler<EditSettingsCommand, SettingsViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async ValueTask<Result<SettingsViewModel>> Handle(EditSettingsCommand req, CancellationToken cancellationToken)
        {
            var existingSettings = await _db.Settings.FirstOrDefaultAsync(cancellationToken);

            if (existingSettings == null)
            {
                return new(CommonErrors.SettingsNotFound(1));
            }

            existingSettings.Discord = req.Discord ?? existingSettings.Discord;
            existingSettings.Steam = req.Steam ?? existingSettings.Steam;
            existingSettings.Patreon = req.Patreon ?? existingSettings.Patreon;
            existingSettings.Github = req.Github ?? existingSettings.Github;
            existingSettings.Reddit = req.Reddit ?? existingSettings.Reddit;
            existingSettings.ModDb = req.ModDb ?? existingSettings.ModDb;
            existingSettings.HappyHours = req.HappyHours ?? existingSettings.HappyHours;

            await _db.SaveChangesAsync(cancellationToken);

            return new(_mapper.Map<SettingsViewModel>(existingSettings));
        }
    }
}

using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Settings;

namespace Crpg.Application.Settings.Models;

public record SettingsViewModel : IMapFrom<Setting>
{
    public string Discord { get; set; } = default!;
    public string Steam { get; set; } = default!;
    public string Patreon { get; set; } = default!;
    public string Github { get; set; } = default!;
    public string Reddit { get; set; } = default!;
    public string ModDb { get; set; } = default!;
    public string HappyHours { get; set; } = default!;
}

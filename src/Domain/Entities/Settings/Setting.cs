namespace Crpg.Domain.Entities.Settings;

public class Setting
{
    public int Id { get; set; } = 1;
    public string Discord { get; set; } = default!;
    public string Steam { get; set; } = default!;
    public string Patreon { get; set; } = default!;
    public string Github { get; set; } = default!;
    public string Reddit { get; set; } = default!;
    public string ModDb { get; set; } = default!;
    public string HappyHours { get; set; } = default!;
}

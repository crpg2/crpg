using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Themes;

public class Theme : AuditableEntity
{
    public Theme(string name)
    {
        Name = name;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    // this contructor exists only because Ef core requires it.
    private Theme()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public int Id { get; set; }

    /// <summary>
    /// The name of the theme.
    /// </summary>
    public string Name { get; set; }
}

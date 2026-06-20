using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Themes;

namespace Crpg.Application.Themes.Models;

public class ThemeViewModel : IMapFrom<Theme>
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
}

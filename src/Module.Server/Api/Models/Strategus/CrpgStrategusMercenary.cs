using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api.Models.Strategus;

internal class CrpgStrategusMercenary
{
    public int Id { get; set; }
    public CrpgUser User { get; set; } = default!;
    public CrpgCharacter Character { get; set; } = default!;
    public BattleSide Side { get; set; }
}

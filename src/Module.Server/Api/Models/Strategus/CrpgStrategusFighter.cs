using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api.Models.Strategus;

internal class CrpgStrategusFighter
{
    public int Id { get; set; }
    public CrpgStrategusParty? Party { get; set; }
    public CrpgStrategusSettlement? Settlement { get; set; }
    public BattleSide Side { get; set; }
    public bool Commander { get; set; }
}

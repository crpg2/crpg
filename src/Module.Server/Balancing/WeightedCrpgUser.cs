using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;

namespace Crpg.Module.Balancing;

internal class WeightedCrpgUser
{
    public WeightedCrpgUser(CrpgUser user, float weight)
    {
        User = user;
        Weight = weight;
    }

    public CrpgUser User { get; }
    public int? ClanId => CrpgServerConfiguration.DisableClanBalancing ? null : User.ClanMembership?.ClanId;
    public float Weight { get; }
}

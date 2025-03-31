using System;
using System.Collections.Generic;
using System.Text;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api.Models.Strategus;

// Copy of Crpg.Application.Parties.Models.PartyPublicViewModel
internal class CrpgStrategusParty
{
    public int Id { get; set; }
    public CrpgUser User { get; set; } = default!;
    public CrpgClan? Clan { get; set; }
}

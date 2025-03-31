using System;
using System.Collections.Generic;
using System.Text;

namespace Crpg.Module.Api.Models.Strategus;

// Copy of Crpg.Application.Settlements.Models.SettlementPublicViewModel
internal class CrpgStrategusSettlement
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CrpgSettlementType Type { get; set; }
    public Point Position { get; set; } = default!;
    public CrpgCulture Culture { get; set; }
    public CrpgRegion Region { get; set; }
}

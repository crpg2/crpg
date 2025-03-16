using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crpg.GameServerManager.Api.Models;
internal class CrpgClaimBattleRequest
{
    public int BattleId { get; set; }
    public string Instance { get; set; } = string.Empty;
}

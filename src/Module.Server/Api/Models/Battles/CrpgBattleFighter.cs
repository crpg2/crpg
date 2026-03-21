using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Api.Models.Battles;

internal enum CrpgBattleSide
{
    Attacker,
    Defender,
}

internal class CrpgBattleFighter
{
    public int Id { get; set; }
    public CrpgBattleSide Side { get; set; }
    public bool Commander { get; set; }
    public int ParticipantSlots { get; set; }
    public CrpgBattleParty? Party { get; set; }
    public CrpgBattleSettlement? Settlement { get; set; }
}

internal class CrpgBattleParty
{
    public int Id { get; set; }
    public int Troops { get; set; }
    public CrpgUser User { get; set; } = null!;
    public IList<CrpgItemStack> Items { get; set; } = [];
}

internal class CrpgBattleSettlement
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Scene { get; set; } = string.Empty;
    public int Troops { get; set; }
    public IList<CrpgItemStack> Items { get; set; } = [];
}

internal class CrpgItemStack
{
    public string ItemId { get; set; } = string.Empty;
    public int Count { get; set; }
}

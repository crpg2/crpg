using Crpg.Module.Api.Models.Items;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgBattleAgentOrigin : BasicBattleAgentOrigin
{
    public MBCharacterSkills Skills { get; }
    public List<(CrpgItemArmorComponent armor, ItemObject.ItemTypeEnum type)> ArmorItems { get; } = new();

    public CrpgBattleAgentOrigin(BasicCharacterObject? troop, MBCharacterSkills skills)
        : base(troop)
    {
        Skills = skills;
    }
}

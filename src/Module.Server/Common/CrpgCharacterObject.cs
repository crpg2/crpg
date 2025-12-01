using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Crpg.Module.Api.Models.Characters;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Common;

public class CrpgCharacterObject : BasicCharacterObject
{
    private readonly MBCharacterSkills _skills;
    private readonly Equipment _equipment;
    public CrpgCharacterObject(MBCharacterSkills skills, Equipment equipment)
    {
        _skills = skills;
        _equipment = equipment;
    }

    public override float Age { get => base.Age; set => base.Age = value; }
    public override bool IsPlayerCharacter => base.IsPlayerCharacter;
    public override bool IsRanged => base.IsRanged;
    public override bool IsHero => base.IsHero;
    public override bool IsMounted => base.IsMounted;
    public override bool IsFemale { get => base.IsFemale; set => base.IsFemale = value; }
    public override int HitPoints => base.HitPoints;
    public override Equipment Equipment => _equipment;
    public override TextObject GetName() => Name;
    protected override MBReadOnlyList<Equipment> AllEquipments => base.AllEquipments;
    public override int GetSkillValue(SkillObject skill)
    {
        return _skills.Skills.GetPropertyValue(skill);
    }

    public override int Level { get => base.Level; set => base.Level = value; }
    public override TextObject Name => new("Crpg Character");
}

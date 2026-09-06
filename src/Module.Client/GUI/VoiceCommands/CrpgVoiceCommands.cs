using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.VoiceCommands;

/// <summary>
/// A node of the voice command menu tree. Either a category (has children) or a leaf bound to a
/// voice type name declared in voice_definitions.xml.
/// </summary>
public class VoiceCommandMenuItem
{
    public VoiceCommandMenuItem(InputKey key, string label, string voiceName)
    {
        Key = key;
        Label = label;
        VoiceName = voiceName;
        Children = Array.Empty<VoiceCommandMenuItem>();
    }

    public VoiceCommandMenuItem(InputKey key, string label, params VoiceCommandMenuItem[] children)
    {
        Key = key;
        Label = label;
        Children = children;
    }

    public InputKey Key { get; }
    public string Label { get; }
    public string? VoiceName { get; }
    public VoiceCommandMenuItem[] Children { get; }
    public bool IsCategory => Children.Length > 0;
}

/// <summary>
/// The voice command menu tree. Hotkeys must be unique within a single page. Directions follow the
/// WASD convention (A = left, D = right, S = behind), e.g. Q I C S = incoming cavalry from the rear.
/// </summary>
public static class CrpgVoiceCommands
{
    public static readonly VoiceCommandMenuItem Root = new(InputKey.Q, "Voice Commands",

        new VoiceCommandMenuItem(InputKey.Q, "Quick",
            new VoiceCommandMenuItem(InputKey.H, "Hello", "QuickHello"),
            new VoiceCommandMenuItem(InputKey.Y, "Yes", "QuickYes"),
            new VoiceCommandMenuItem(InputKey.N, "No", "QuickNo"),
            new VoiceCommandMenuItem(InputKey.Q, "Help", "QuickHelp"),
            new VoiceCommandMenuItem(InputKey.M, "Move", "QuickMove"),
            new VoiceCommandMenuItem(InputKey.S, "Silence", "QuickSilence"),
            new VoiceCommandMenuItem(InputKey.A, "Sorry", "QuickSorry"),
            new VoiceCommandMenuItem(InputKey.X, "Stop", "QuickStop"),
            new VoiceCommandMenuItem(InputKey.T, "Thanks", "QuickThanks"),
            new VoiceCommandMenuItem(InputKey.W, "Watch your aim", "QuickWatchAim"),
            new VoiceCommandMenuItem(InputKey.V, "Yell", "QuickYell"),
            new VoiceCommandMenuItem(InputKey.F, "Whistle", "QuickWhistle")),

        new VoiceCommandMenuItem(InputKey.A, "Attack",
            new VoiceCommandMenuItem(InputKey.A, "Attack", "AttackAttack"),
            new VoiceCommandMenuItem(InputKey.X, "Charge", "AttackCharge"),
            new VoiceCommandMenuItem(InputKey.Q, "Attack left flank!", "AttackLeftFlank"),
            new VoiceCommandMenuItem(InputKey.E, "Attack right flank!", "AttackRightFlank"),
            new VoiceCommandMenuItem(InputKey.R, "Attack their archers!", "AttackArchers"),
            new VoiceCommandMenuItem(InputKey.C, "Attack their cavalry!", "AttackCavalry"),
            new VoiceCommandMenuItem(InputKey.I, "Attack their infantry!", "AttackInfantry"),
            new VoiceCommandMenuItem(InputKey.H, "Attack the catapult!", "AttackCatapult"),
            new VoiceCommandMenuItem(InputKey.J, "Attack the siege tower!", "AttackSiegeTower"),
            new VoiceCommandMenuItem(InputKey.P, "Attack their equipment!", "AttackEquipment"),
            new VoiceCommandMenuItem(InputKey.B, "Attack their base! ", "AttackBase"),
            new VoiceCommandMenuItem(InputKey.G, "Open the gate!", "AttackGate"),
            new VoiceCommandMenuItem(InputKey.T, "Attack the tower!", "AttackTower")),

        new VoiceCommandMenuItem(InputKey.D, "Defend",
            new VoiceCommandMenuItem(InputKey.D, "Defend", "DefendDefend"),
            new VoiceCommandMenuItem(InputKey.W, "Defend the walls!", "DefendWalls"),
            new VoiceCommandMenuItem(InputKey.V, "Defend the gatehouse!", "DefendGatehouse"),
            new VoiceCommandMenuItem(InputKey.F, "Defend the flag!", "DefendFlag"),
            new VoiceCommandMenuItem(InputKey.G, "Defend the gate!", "DefendGate"),
            new VoiceCommandMenuItem(InputKey.H, "Defend the catapult!", "DefendCatapult"),
            new VoiceCommandMenuItem(InputKey.S, "Defend the siege tower!", "DefendSiegeTower"),
            new VoiceCommandMenuItem(InputKey.C, "Defend the flag carrier!", "DefendFlagCarrier"),
            new VoiceCommandMenuItem(InputKey.L, "Defend the ladders!", "DefendLadders"),
            new VoiceCommandMenuItem(InputKey.S, "Defend the stairs!", "DefendStairs"),
            new VoiceCommandMenuItem(InputKey.T, "Defend the tower!", "DefendTower"),
            new VoiceCommandMenuItem(InputKey.X, "Close the gate!", "DefendCloseGate")),

        new VoiceCommandMenuItem(InputKey.I, "Incoming",
            new VoiceCommandMenuItem(InputKey.C, "Incoming cavalry",
                new VoiceCommandMenuItem(InputKey.C, "Incoming cavalry!", "IncomingCavalry"),
                new VoiceCommandMenuItem(InputKey.Q, "Incoming cavalry from the left!", "IncomingCavalryLeft"),
                new VoiceCommandMenuItem(InputKey.E, "Incoming cavalry from the right!", "IncomingCavalryRight"),
                new VoiceCommandMenuItem(InputKey.S, "Incoming cavalry from behind!", "IncomingCavalryBehind")),

            new VoiceCommandMenuItem(InputKey.I, "Incoming infantry",
                new VoiceCommandMenuItem(InputKey.I, "Incoming infantry!", "IncomingInfantry"),
                new VoiceCommandMenuItem(InputKey.Q, "Incoming infantry from the left!", "IncomingInfantryLeft"),
                new VoiceCommandMenuItem(InputKey.E, "Incoming infantry from the right!", "IncomingInfantryRight"),
                new VoiceCommandMenuItem(InputKey.S, "Incoming infantry from behind!", "IncomingInfantryBehind")),

            new VoiceCommandMenuItem(InputKey.A, "Arrows incoming!", "IncomingArrows"),
            new VoiceCommandMenuItem(InputKey.H, "Incoming catapult", "IncomingCatapult"),
            new VoiceCommandMenuItem(InputKey.J, "Incoming siege tower", "IncomingSiegeTower"),
            new VoiceCommandMenuItem(InputKey.X, "Enemies inside the base!", "IncomingBase")),

        new VoiceCommandMenuItem(InputKey.E, "Equipment",
            new VoiceCommandMenuItem(InputKey.H, "Deploy a catapult!", "EquipmentDeployCatapult"),
            new VoiceCommandMenuItem(InputKey.N, "Man the catapult!", "EquipmentManCatapult"),
            new VoiceCommandMenuItem(InputKey.J, "Build a siege tower!", "EquipmentBuildSiegeTower"),
            new VoiceCommandMenuItem(InputKey.N, "Man the siege tower!", "EquipmentManSiegeTower"),
            new VoiceCommandMenuItem(InputKey.L, "Deploy ladders!", "EquipmentLadders"),
            new VoiceCommandMenuItem(InputKey.S, "Deploy shields!", "EquipmentShields"),
            new VoiceCommandMenuItem(InputKey.E, "Deploy equipment!", "EquipmentEquipment")),

         new VoiceCommandMenuItem(InputKey.S, "Self",
            new VoiceCommandMenuItem(InputKey.A, "Attacking",
                new VoiceCommandMenuItem(InputKey.A, "I am attacking", "SelfAttackAttacking"),
                new VoiceCommandMenuItem(InputKey.Q, "I am attacking left", "SelfAttackLeft"),
                new VoiceCommandMenuItem(InputKey.E, "I am attacking right", "SelfAttackRight"),
                new VoiceCommandMenuItem(InputKey.C, "I am attacking the catapult", "SelfAttackCatapult"),
                new VoiceCommandMenuItem(InputKey.J, "I am attacking the siege tower", "SelfAttackSiegeTower"),
                new VoiceCommandMenuItem(InputKey.R, "I am attacking their equipment", "SelfAttackEquipment"),
                new VoiceCommandMenuItem(InputKey.B, "I am attacking their base", "SelfAttackBase"),
                new VoiceCommandMenuItem(InputKey.G, "I am opening the gate", "SelfAttackGate"),
                new VoiceCommandMenuItem(InputKey.T, "I am attacking the tower", "SelfAttackTower"),
                new VoiceCommandMenuItem(InputKey.F, "I am going for the flag", "SelfAttackFlag")),

            new VoiceCommandMenuItem(InputKey.D, "Defending",
                new VoiceCommandMenuItem(InputKey.W, "I am defending the walls", "SelfDefendWalls"),
                new VoiceCommandMenuItem(InputKey.V, "I am defending the gatehouse", "SelfDefendGatehouse"),
                new VoiceCommandMenuItem(InputKey.F, "I am defending the flag", "SelfDefendFlag"),
                new VoiceCommandMenuItem(InputKey.G, "I am defending the gate", "SelfDefendGate"),
                new VoiceCommandMenuItem(InputKey.H, "I am defending the catapult", "SelfDefendCatapult"),
                new VoiceCommandMenuItem(InputKey.T, "I am defending the siege tower", "SelfDefendSiegeTower"),
                new VoiceCommandMenuItem(InputKey.L, "I am defending the ladders", "SelfDefendLadders"),
                new VoiceCommandMenuItem(InputKey.S, "I am defending the stairs", "SelfDefendStairs"),
                new VoiceCommandMenuItem(InputKey.J, "I am defending the tower", "SelfDefendTower"),
                new VoiceCommandMenuItem(InputKey.C, "I am closing the gate", "SelfDefendCloseGate")),

            new VoiceCommandMenuItem(InputKey.E, "Equipment",
                new VoiceCommandMenuItem(InputKey.B, "I am building a catapult", "SelfEquipmentBuildCatapult"),
                new VoiceCommandMenuItem(InputKey.N, "I am manning the catapult", "SelfEquipmentManCatapult"),
                new VoiceCommandMenuItem(InputKey.H, "I need help at the catapult", "SelfEquipmentHelpCatapult"),
                new VoiceCommandMenuItem(InputKey.I, "I am building a siege tower", "SelfEquipmentBuildSiegeTower"),
                new VoiceCommandMenuItem(InputKey.M, "I am manning the siege tower", "SelfEquipmentManSiegeTower"),
                new VoiceCommandMenuItem(InputKey.J, "I need help at the siege tower", "SelfEquipmentHelpSiegeTower"),
                new VoiceCommandMenuItem(InputKey.L, "I am deploying ladders", "SelfEquipmentLadders"),
                new VoiceCommandMenuItem(InputKey.S, "I am deploying shields", "SelfEquipmentShields"),
                new VoiceCommandMenuItem(InputKey.E, "I am deploying equipment", "SelfEquipmentEquipment"))),

    new VoiceCommandMenuItem(InputKey.F, "Formation",
            new VoiceCommandMenuItem(InputKey.R, "Archers!", "FormationArchers"),
            new VoiceCommandMenuItem(InputKey.I, "Infantry!", "FormationInfantry"),
            new VoiceCommandMenuItem(InputKey.C, "Cavalry!", "FormationCavalry"),
            new VoiceCommandMenuItem(InputKey.H, "Hold!", "FormationHold"),
            new VoiceCommandMenuItem(InputKey.F, "Keep formation", "FormationKeep"),
            new VoiceCommandMenuItem(InputKey.Y, "Retreat!", "FormationRetreat"),
            new VoiceCommandMenuItem(InputKey.X, "Fall back!", "FormationFallBack"),
            new VoiceCommandMenuItem(InputKey.S, "Shoot!", "FormationShoot"),
            new VoiceCommandMenuItem(InputKey.A, "Release arrows!", "FormationReleaseArrows"),
            new VoiceCommandMenuItem(InputKey.T, "Take cover!", "FormationTakeCover"),
            new VoiceCommandMenuItem(InputKey.W, "Shieldwall!", "FormationShieldwall"),
            new VoiceCommandMenuItem(InputKey.D, "Stand closer!", "FormationStandCloser"),
            new VoiceCommandMenuItem(InputKey.E, "Spread out!", "FormationSpreadOut"),
            new VoiceCommandMenuItem(InputKey.M, "Follow me!", "FormationFollowMe")));


    private static SkinVoiceManager.SkinVoiceType[]? _allLeafVoices;

    public static SkinVoiceManager.SkinVoiceType GetRandomVoice()
    {
        if (_allLeafVoices == null)
        {
            var leaves = new List<SkinVoiceManager.SkinVoiceType>();
            Collect(Root, leaves);
            _allLeafVoices = leaves.ToArray();
        }

        return _allLeafVoices.GetRandomElement();
    }

    private static void Collect(VoiceCommandMenuItem node, List<SkinVoiceManager.SkinVoiceType> leaves)
    {
        if (!node.IsCategory)
        {
            leaves.Add(new SkinVoiceManager.SkinVoiceType(node.VoiceName!));
            return;
        }

        foreach (var child in node.Children)
        {
            Collect(child, leaves);
        }
    }
}

using Crpg.Module.Common.VoiceCommands;
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
    public VoiceCommandMenuItem(InputKey key, string label, CrpgVoiceCommand voiceCommand)
    {
        Key = key;
        Label = label;
        VoiceCommand = voiceCommand;
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
    public CrpgVoiceCommand? VoiceCommand { get; }
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
            new VoiceCommandMenuItem(InputKey.H, "Hello", CrpgVoiceCommand.QuickHello),
            new VoiceCommandMenuItem(InputKey.Y, "Yes", CrpgVoiceCommand.QuickYes),
            new VoiceCommandMenuItem(InputKey.N, "No", CrpgVoiceCommand.QuickNo),
            new VoiceCommandMenuItem(InputKey.Q, "Help", CrpgVoiceCommand.QuickHelp),
            new VoiceCommandMenuItem(InputKey.M, "Move", CrpgVoiceCommand.QuickMove),
            new VoiceCommandMenuItem(InputKey.S, "Silence", CrpgVoiceCommand.QuickSilence),
            new VoiceCommandMenuItem(InputKey.A, "Sorry", CrpgVoiceCommand.QuickSorry),
            new VoiceCommandMenuItem(InputKey.X, "Stop", CrpgVoiceCommand.QuickStop),
            new VoiceCommandMenuItem(InputKey.T, "Thanks", CrpgVoiceCommand.QuickThanks),
            new VoiceCommandMenuItem(InputKey.W, "Watch your aim", CrpgVoiceCommand.QuickWatchAim),
            new VoiceCommandMenuItem(InputKey.V, "Yell", CrpgVoiceCommand.QuickYell),
            new VoiceCommandMenuItem(InputKey.F, "Whistle", CrpgVoiceCommand.QuickWhistle)),

        new VoiceCommandMenuItem(InputKey.A, "Attack",
            new VoiceCommandMenuItem(InputKey.A, "Attack", CrpgVoiceCommand.AttackAttack),
            new VoiceCommandMenuItem(InputKey.X, "Charge", CrpgVoiceCommand.AttackCharge),
            new VoiceCommandMenuItem(InputKey.Q, "Attack left flank!", CrpgVoiceCommand.AttackLeftFlank),
            new VoiceCommandMenuItem(InputKey.E, "Attack right flank!", CrpgVoiceCommand.AttackRightFlank),
            new VoiceCommandMenuItem(InputKey.R, "Attack their archers!", CrpgVoiceCommand.AttackArchers),
            new VoiceCommandMenuItem(InputKey.C, "Attack their cavalry!", CrpgVoiceCommand.AttackCavalry),
            new VoiceCommandMenuItem(InputKey.I, "Attack their infantry!", CrpgVoiceCommand.AttackInfantry),
            new VoiceCommandMenuItem(InputKey.H, "Attack the catapult!", CrpgVoiceCommand.AttackCatapult),
            new VoiceCommandMenuItem(InputKey.J, "Attack the siege tower!", CrpgVoiceCommand.AttackSiegeTower),
            new VoiceCommandMenuItem(InputKey.P, "Attack their equipment!", CrpgVoiceCommand.AttackEquipment),
            new VoiceCommandMenuItem(InputKey.B, "Attack their base! ", CrpgVoiceCommand.AttackBase),
            new VoiceCommandMenuItem(InputKey.G, "Open the gate!", CrpgVoiceCommand.AttackGate),
            new VoiceCommandMenuItem(InputKey.T, "Attack the tower!", CrpgVoiceCommand.AttackTower)),

        new VoiceCommandMenuItem(InputKey.D, "Defend",
            new VoiceCommandMenuItem(InputKey.D, "Defend", CrpgVoiceCommand.DefendDefend),
            new VoiceCommandMenuItem(InputKey.W, "Defend the walls!", CrpgVoiceCommand.DefendWalls),
            new VoiceCommandMenuItem(InputKey.V, "Defend the gatehouse!", CrpgVoiceCommand.DefendGatehouse),
            new VoiceCommandMenuItem(InputKey.F, "Defend the flag!", CrpgVoiceCommand.DefendFlag),
            new VoiceCommandMenuItem(InputKey.G, "Defend the gate!", CrpgVoiceCommand.DefendGate),
            new VoiceCommandMenuItem(InputKey.H, "Defend the catapult!", CrpgVoiceCommand.DefendCatapult),
            new VoiceCommandMenuItem(InputKey.J, "Defend the siege tower!", CrpgVoiceCommand.DefendSiegeTower),
            new VoiceCommandMenuItem(InputKey.C, "Defend the flag carrier!", CrpgVoiceCommand.DefendFlagCarrier),
            new VoiceCommandMenuItem(InputKey.L, "Defend the ladders!", CrpgVoiceCommand.DefendLadders),
            new VoiceCommandMenuItem(InputKey.S, "Defend the stairs!", CrpgVoiceCommand.DefendStairs),
            new VoiceCommandMenuItem(InputKey.T, "Defend the tower!", CrpgVoiceCommand.DefendTower),
            new VoiceCommandMenuItem(InputKey.X, "Close the gate!", CrpgVoiceCommand.DefendCloseGate)),

        new VoiceCommandMenuItem(InputKey.I, "Incoming",
            new VoiceCommandMenuItem(InputKey.C, "Incoming cavalry",
                new VoiceCommandMenuItem(InputKey.C, "Incoming cavalry!", CrpgVoiceCommand.IncomingCavalry),
                new VoiceCommandMenuItem(InputKey.Q, "Incoming cavalry from the left!", CrpgVoiceCommand.IncomingCavalryLeft),
                new VoiceCommandMenuItem(InputKey.E, "Incoming cavalry from the right!", CrpgVoiceCommand.IncomingCavalryRight),
                new VoiceCommandMenuItem(InputKey.S, "Incoming cavalry from behind!", CrpgVoiceCommand.IncomingCavalryBehind)),

            new VoiceCommandMenuItem(InputKey.I, "Incoming infantry",
                new VoiceCommandMenuItem(InputKey.I, "Incoming infantry!", CrpgVoiceCommand.IncomingInfantry),
                new VoiceCommandMenuItem(InputKey.Q, "Incoming infantry from the left!", CrpgVoiceCommand.IncomingInfantryLeft),
                new VoiceCommandMenuItem(InputKey.E, "Incoming infantry from the right!", CrpgVoiceCommand.IncomingInfantryRight),
                new VoiceCommandMenuItem(InputKey.S, "Incoming infantry from behind!", CrpgVoiceCommand.IncomingInfantryBehind)),

            new VoiceCommandMenuItem(InputKey.A, "Arrows incoming!", CrpgVoiceCommand.IncomingArrows),
            new VoiceCommandMenuItem(InputKey.H, "Incoming catapult", CrpgVoiceCommand.IncomingCatapult),
            new VoiceCommandMenuItem(InputKey.J, "Incoming siege tower", CrpgVoiceCommand.IncomingSiegeTower),
            new VoiceCommandMenuItem(InputKey.X, "Enemies inside the base!", CrpgVoiceCommand.IncomingBase)),

        new VoiceCommandMenuItem(InputKey.E, "Equipment",
            new VoiceCommandMenuItem(InputKey.H, "Deploy a catapult!", CrpgVoiceCommand.EquipmentDeployCatapult),
            new VoiceCommandMenuItem(InputKey.N, "Man the catapult!", CrpgVoiceCommand.EquipmentManCatapult),
            new VoiceCommandMenuItem(InputKey.J, "Build a siege tower!", CrpgVoiceCommand.EquipmentBuildSiegeTower),
            new VoiceCommandMenuItem(InputKey.N, "Man the siege tower!", CrpgVoiceCommand.EquipmentManSiegeTower),
            new VoiceCommandMenuItem(InputKey.L, "Deploy ladders!", CrpgVoiceCommand.EquipmentLadders),
            new VoiceCommandMenuItem(InputKey.S, "Deploy shields!", CrpgVoiceCommand.EquipmentShields),
            new VoiceCommandMenuItem(InputKey.E, "Deploy equipment!", CrpgVoiceCommand.EquipmentEquipment)),

         new VoiceCommandMenuItem(InputKey.S, "Self",
            new VoiceCommandMenuItem(InputKey.A, "Attacking",
                new VoiceCommandMenuItem(InputKey.A, "I am attacking", CrpgVoiceCommand.SelfAttackAttacking),
                new VoiceCommandMenuItem(InputKey.Q, "I am attacking left", CrpgVoiceCommand.SelfAttackLeft),
                new VoiceCommandMenuItem(InputKey.E, "I am attacking right", CrpgVoiceCommand.SelfAttackRight),
                new VoiceCommandMenuItem(InputKey.C, "I am attacking the catapult", CrpgVoiceCommand.SelfAttackCatapult),
                new VoiceCommandMenuItem(InputKey.J, "I am attacking the siege tower", CrpgVoiceCommand.SelfAttackSiegeTower),
                new VoiceCommandMenuItem(InputKey.R, "I am attacking their equipment", CrpgVoiceCommand.SelfAttackEquipment),
                new VoiceCommandMenuItem(InputKey.B, "I am attacking their base", CrpgVoiceCommand.SelfAttackBase),
                new VoiceCommandMenuItem(InputKey.G, "I am opening the gate", CrpgVoiceCommand.SelfAttackGate),
                new VoiceCommandMenuItem(InputKey.T, "I am attacking the tower", CrpgVoiceCommand.SelfAttackTower),
                new VoiceCommandMenuItem(InputKey.F, "I am going for the flag", CrpgVoiceCommand.SelfAttackFlag)),

            new VoiceCommandMenuItem(InputKey.D, "Defending",
                new VoiceCommandMenuItem(InputKey.W, "I am defending the walls", CrpgVoiceCommand.SelfDefendWalls),
                new VoiceCommandMenuItem(InputKey.V, "I am defending the gatehouse", CrpgVoiceCommand.SelfDefendGatehouse),
                new VoiceCommandMenuItem(InputKey.F, "I am defending the flag", CrpgVoiceCommand.SelfDefendFlag),
                new VoiceCommandMenuItem(InputKey.G, "I am defending the gate", CrpgVoiceCommand.SelfDefendGate),
                new VoiceCommandMenuItem(InputKey.H, "I am defending the catapult", CrpgVoiceCommand.SelfDefendCatapult),
                new VoiceCommandMenuItem(InputKey.T, "I am defending the siege tower", CrpgVoiceCommand.SelfDefendSiegeTower),
                new VoiceCommandMenuItem(InputKey.L, "I am defending the ladders", CrpgVoiceCommand.SelfDefendLadders),
                new VoiceCommandMenuItem(InputKey.S, "I am defending the stairs", CrpgVoiceCommand.SelfDefendStairs),
                new VoiceCommandMenuItem(InputKey.J, "I am defending the tower", CrpgVoiceCommand.SelfDefendTower),
                new VoiceCommandMenuItem(InputKey.C, "I am closing the gate", CrpgVoiceCommand.SelfDefendCloseGate)),

            new VoiceCommandMenuItem(InputKey.E, "Equipment",
                new VoiceCommandMenuItem(InputKey.B, "I am building a catapult", CrpgVoiceCommand.SelfEquipmentBuildCatapult),
                new VoiceCommandMenuItem(InputKey.N, "I am manning the catapult", CrpgVoiceCommand.SelfEquipmentManCatapult),
                new VoiceCommandMenuItem(InputKey.H, "I need help at the catapult", CrpgVoiceCommand.SelfEquipmentHelpCatapult),
                new VoiceCommandMenuItem(InputKey.I, "I am building a siege tower", CrpgVoiceCommand.SelfEquipmentBuildSiegeTower),
                new VoiceCommandMenuItem(InputKey.M, "I am manning the siege tower", CrpgVoiceCommand.SelfEquipmentManSiegeTower),
                new VoiceCommandMenuItem(InputKey.J, "I need help at the siege tower", CrpgVoiceCommand.SelfEquipmentHelpSiegeTower),
                new VoiceCommandMenuItem(InputKey.L, "I am deploying ladders", CrpgVoiceCommand.SelfEquipmentLadders),
                new VoiceCommandMenuItem(InputKey.S, "I am deploying shields", CrpgVoiceCommand.SelfEquipmentShields),
                new VoiceCommandMenuItem(InputKey.E, "I am deploying equipment", CrpgVoiceCommand.SelfEquipmentEquipment))),

    new VoiceCommandMenuItem(InputKey.F, "Formation",
            new VoiceCommandMenuItem(InputKey.R, "Archers!", CrpgVoiceCommand.FormationArchers),
            new VoiceCommandMenuItem(InputKey.I, "Infantry!", CrpgVoiceCommand.FormationInfantry),
            new VoiceCommandMenuItem(InputKey.C, "Cavalry!", CrpgVoiceCommand.FormationCavalry),
            new VoiceCommandMenuItem(InputKey.H, "Hold!", CrpgVoiceCommand.FormationHold),
            new VoiceCommandMenuItem(InputKey.F, "Keep formation", CrpgVoiceCommand.FormationKeep),
            new VoiceCommandMenuItem(InputKey.Y, "Retreat!", CrpgVoiceCommand.FormationRetreat),
            new VoiceCommandMenuItem(InputKey.X, "Fall back!", CrpgVoiceCommand.FormationFallBack),
            new VoiceCommandMenuItem(InputKey.S, "Shoot!", CrpgVoiceCommand.FormationShoot),
            new VoiceCommandMenuItem(InputKey.A, "Release arrows!", CrpgVoiceCommand.FormationReleaseArrows),
            new VoiceCommandMenuItem(InputKey.T, "Take cover!", CrpgVoiceCommand.FormationTakeCover),
            new VoiceCommandMenuItem(InputKey.W, "Shieldwall!", CrpgVoiceCommand.FormationShieldwall),
            new VoiceCommandMenuItem(InputKey.D, "Stand closer!", CrpgVoiceCommand.FormationStandCloser),
            new VoiceCommandMenuItem(InputKey.E, "Spread out!", CrpgVoiceCommand.FormationSpreadOut),
            new VoiceCommandMenuItem(InputKey.M, "Follow me!", CrpgVoiceCommand.FormationFollowMe)));


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
            CrpgVoiceCommand voiceCommand = node.VoiceCommand ?? throw new InvalidOperationException("Voice command leaf has no command.");
            leaves.Add(new SkinVoiceManager.SkinVoiceType(CrpgVoiceCommandCatalog.GetVoiceName((int)voiceCommand)));
            return;
        }

        foreach (var child in node.Children)
        {
            Collect(child, leaves);
        }
    }
}

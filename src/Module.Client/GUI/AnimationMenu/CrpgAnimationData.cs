namespace Crpg.Module.GUI.AnimationMenu;

/// <summary>
/// A single emote entry that maps a display name to an engine action id.
/// ZOffset / YOffset teleport the agent on play (useful for sitting poses).
/// StartProgress lets the animation start mid-way (0.0–0.89).
/// Speed controls playback rate (0.0 = freeze).
/// </summary>
internal sealed class CrpgEmoteEntry
{
    public string ActionId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public float ZOffset { get; init; } = 0f;
    public float YOffset { get; init; } = 0f;
    public float StartProgress { get; init; } = 0f;
    public float Speed { get; init; } = 1f;
}

/// <summary>A named group of emote entries shown as a submenu page.</summary>
internal sealed class CrpgEmoteCategory
{
    public string DisplayName { get; init; } = string.Empty;
    public IReadOnlyList<CrpgEmoteEntry> Emotes { get; init; } = Array.Empty<CrpgEmoteEntry>();
}

/// <summary>
/// Static catalogue of all emotes available in cRPG.
/// Only vanilla (Native / Multiplayer) animations are used — no extra .tpac required.
/// Action ids must match crpg_action_types.xml and crpg_action_sets.xml.
/// </summary>
internal static class CrpgAnimationData
{
    public static readonly IReadOnlyList<CrpgEmoteCategory> Categories = new CrpgEmoteCategory[]
    {
        new()
        {
            DisplayName = "Idle",
            Emotes = new CrpgEmoteEntry[]
            {
                new() { ActionId = "crpg_act_stand_1", DisplayName = "Hands Behind" },
                new() { ActionId = "crpg_act_stand_2", DisplayName = "Idle Pose 2" },
                new() { ActionId = "crpg_act_stand_3", DisplayName = "Arms on Sides" },
                new() { ActionId = "crpg_act_stand_4", DisplayName = "Arms Crossed" },
                new() { ActionId = "crpg_act_stand_5", DisplayName = "Hands on Hips" },
                new() { ActionId = "crpg_act_stand_6", DisplayName = "In Thought" },
            },
        },
        new()
        {
            DisplayName = "Sitting",
            Emotes = new CrpgEmoteEntry[]
            {
                new() { ActionId = "crpg_act_sit_drink_1", DisplayName = "Drinking 1", ZOffset = -0.5f },
                new() { ActionId = "crpg_act_sit_drink_2", DisplayName = "Drinking 2", ZOffset = -0.5f },
                new() { ActionId = "crpg_act_sit_drink_3", DisplayName = "Drinking 3", ZOffset = -0.5f },
            },
        },
        new()
        {
            DisplayName = "Taunts",
            Emotes = new CrpgEmoteEntry[]
            {
                new() { ActionId = "crpg_act_taunt_afraid",      DisplayName = "Afraid" },
                new() { ActionId = "crpg_act_taunt_bow",         DisplayName = "Bow" },
                new() { ActionId = "crpg_act_taunt_break",       DisplayName = "Break" },
                new() { ActionId = "crpg_act_taunt_disappointed",DisplayName = "Disappointed" },
                new() { ActionId = "crpg_act_taunt_hit_shield_2",DisplayName = "Hit Shield" },
                new() { ActionId = "crpg_act_taunt_invite",      DisplayName = "Invite" },
                new() { ActionId = "crpg_act_taunt_invite_2",    DisplayName = "Invite 2" },
                new() { ActionId = "crpg_act_taunt_laugh",       DisplayName = "Laugh" },
                new() { ActionId = "crpg_act_taunt_pointing",    DisplayName = "Pointing" },
                new() { ActionId = "crpg_act_taunt_rage",        DisplayName = "Rage" },
                new() { ActionId = "crpg_act_taunt_respect",     DisplayName = "Respect" },
                new() { ActionId = "crpg_act_taunt_scared",      DisplayName = "Scared" },
                new() { ActionId = "crpg_act_taunt_surrender",   DisplayName = "Surrender" },
                new() { ActionId = "crpg_act_taunt_threat",      DisplayName = "Threat" },
                new() { ActionId = "crpg_act_taunt_threat_2",    DisplayName = "Threat 2" },
            },
        },
        new()
        {
            DisplayName = "Cheers",
            Emotes = new CrpgEmoteEntry[]
            {
                new() { ActionId = "crpg_act_cheer_01", DisplayName = "Cheer 1" },
                new() { ActionId = "crpg_act_cheer_02", DisplayName = "Cheer 2" },
                new() { ActionId = "crpg_act_cheer_03", DisplayName = "Cheer 3" },
                new() { ActionId = "crpg_act_cheer_04", DisplayName = "Cheer 4" },
                new() { ActionId = "crpg_act_cheer_05", DisplayName = "Cheer 5" },
                new() { ActionId = "crpg_act_cheer_06", DisplayName = "Cheer 6" },
                new() { ActionId = "crpg_act_cheer_07", DisplayName = "Cheer 7" },
                new() { ActionId = "crpg_act_cheer_08", DisplayName = "Cheer 8" },
            },
        },
        new()
        {
            DisplayName = "Misc",
            Emotes = new CrpgEmoteEntry[]
            {
                new() { ActionId = "crpg_act_dance_norse", DisplayName = "Norse Dance" },
            },
        },
    };
}

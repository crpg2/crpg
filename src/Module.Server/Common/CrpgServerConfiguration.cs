using System.Text.RegularExpressions;
using Crpg.Module.Api.Models;
using Crpg.Module.HarmonyPatches;
using JetBrains.Annotations;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal static class CrpgServerConfiguration
{
    static CrpgServerConfiguration()
    {
        string? regionStr = Environment.GetEnvironmentVariable("CRPG_REGION");
        Region = Enum.TryParse(regionStr, ignoreCase: true, out CrpgRegion region) ? region : CrpgRegion.Eu;
        Service = Environment.GetEnvironmentVariable("CRPG_SERVICE") ?? "unknown-service";
        Instance = Environment.GetEnvironmentVariable("CRPG_INSTANCE") ?? "unknown-instance";
    }

    public static void Init()
    {
        DedicatedServerConsoleCommandManager.AddType(typeof(CrpgServerConfiguration));
    }

    public static CrpgRegion Region { get; }
    public static string Service { get; }
    public static string Instance { get; }
    public static float TeamBalancerClanGroupSizePenalty { get; private set; } = 0f;
    public static float ServerExperienceMultiplier { get; private set; } = 1.0f;
    public static int RewardTick { get; private set; } = 60;
    public static bool TeamBalanceOnce { get; private set; }
    public static bool FrozenBots { get; private set; } = false;
    public static int ControlledBotsCount { get; private set; } = 0;
    public static int BaseNakedEquipmentValue { get; private set; } = 10000;
    public static Tuple<TimeSpan, TimeSpan, TimeZoneInfo>? HappyHours { get; private set; }
    public static bool DisableAllChargeDamage { get; set; } = false;
    public static bool AllowFriendlyChargeDamage { get; set; } = true;
    public static bool AllowChargeEnemies { get; set; } = true;
    public static bool MirrorFriendlyChargeDamageMount { get; set; } = true;
    public static bool MirrorFriendlyChargeDamageAgent { get; set; } = false;
    public static int MirrorMountDamageMultiplier { get; set; } = 5;
    public static int MirrorAgentDamageMultiplier { get; set; } = 1;
    public static int MirrorMountDamageMaximum { get; set; } = 100;
    public static int MirrorMountDamageMinimum { get; set; } = 0;
    public static int MirrorMountDamageMaximumPercentage { get; set; } = 50;
    public static float MinimumChargeVelocityForFriendlyDamage { get; set; } = 0.0f;

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_team_balancer_clan_group_size_penalty", "Apply a rating increase to members of the same clan that are playing in the same team")]
    private static void SetClanGroupSizePenalty(string? sizePenaltyStr)
    {
        if (sizePenaltyStr == null
            || !float.TryParse(sizePenaltyStr, out float sizePenalty)
            || sizePenalty > 1.5f)
        {
            Debug.Print($"Invalid team balancer clangroup size penalty: {sizePenaltyStr}");
            return;
        }

        TeamBalancerClanGroupSizePenalty = sizePenalty;
        Debug.Print($"Set ClanGroup Size Penalty to {sizePenalty}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_experience_multiplier", "Sets a reward multiplier for the server.")]
    private static void SetServerExperienceMultiplier(string? multiplierStr)
    {
        if (multiplierStr == null
            || !float.TryParse(multiplierStr, out float multiplier)
            || multiplier > 5f)
        {
            Debug.Print($"Invalid server multiplier: {multiplierStr}");
            return;
        }

        ServerExperienceMultiplier = multiplier;
        Debug.Print($"Set server multiplier to {multiplier}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_base_naked_equipment_value", "Sets the equipment value of a naked character. used to combat dagger builds in captain.")]
    private static void SetBaseNakedEquipmentValue(string? baseNakedEquipmentValueStr)
    {
        if (baseNakedEquipmentValueStr == null || !int.TryParse(baseNakedEquipmentValueStr, out int baseNakedEquipmentValue))
        {
            Debug.Print($"Invalid Controlled Bots Count: {baseNakedEquipmentValueStr}");
            return;
        }

        ControlledBotsCount = baseNakedEquipmentValue;
        Debug.Print($"Sets baseNakedEquipmentValue to {baseNakedEquipmentValue}");
    }

    [ConsoleCommandMethod("crpg_controlled_bots_count", "Sets ControlledBotsCount in captain  and battle.")]
    private static void SetControlledBotsCount(string? botsCountStr)
    {
        if (botsCountStr == null || !int.TryParse(botsCountStr, out int botsCount))
        {
            Debug.Print($"Invalid Controlled Bots Count: {botsCountStr}");
            return;
        }

        ControlledBotsCount = botsCount;
        Debug.Print($"Sets ControlledBotsCount to {botsCount}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_reward_tick", "Sets the reward tick duration in seconds for Conquest/Siege/Team Deatmatch.")]
    private static void SetRewardTick(string? rewardTickStr)
    {
        if (rewardTickStr == null
            || !int.TryParse(rewardTickStr, out int rewardTick)
            || rewardTick < 10
            || rewardTick > 1000)
        {
            Debug.Print($"Invalid reward tick: {rewardTickStr}");
            return;
        }

        RewardTick = rewardTick;
        Debug.Print($"Set reward tick to {rewardTick}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_team_balance_once", "Sets if the team balancer should balance only after warmup.")]
    private static void SetTeamBalanceOnce(string? teamBalanceOnceStr)
    {
        if (teamBalanceOnceStr == null
            || !bool.TryParse(teamBalanceOnceStr, out bool teamBalanceOnce))
        {
            Debug.Print($"Invalid team balance once: {teamBalanceOnceStr}");
            return;
        }

        TeamBalanceOnce = teamBalanceOnce;
        Debug.Print($"Set team balance once to {teamBalanceOnce}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_frozen_bots", "Sets the Alarmed status of bots to off.")]
    private static void SetFrozenBots(string? frozenBotsStr)
    {
        if (frozenBotsStr == null
            || !bool.TryParse(frozenBotsStr, out bool frozenBots))
        {
            Debug.Print($"Invalid Frozen Bots: {frozenBotsStr}");
            return;
        }

        FrozenBots = frozenBots;
        Debug.Print($"Set team balance once to {frozenBots}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_happy_hours", "Sets the happy hours. Format: HH:MM-HH:MM,TZ")]
    private static void SetHappyHours(string? happHoursStr)
    {
        if (happHoursStr == null)
        {
            Debug.Print("Invalid happy hours: null");
            return;
        }

        Match match = Regex.Match(happHoursStr, "^(\\d\\d:\\d\\d)-(\\d\\d:\\d\\d),([\\w/ ]+)$");
        if (match.Groups.Count != 4
            || !TimeSpan.TryParse(match.Groups[1].Value, out var startTime)
            || startTime < TimeSpan.Zero
            || startTime > TimeSpan.FromHours(24)
            || !TimeSpan.TryParse(match.Groups[2].Value, out var endTime)
            || endTime < TimeSpan.Zero
            || endTime > TimeSpan.FromHours(24))
        {
            Debug.Print($"Invalid happy hours: {happHoursStr}");
            return;
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(match.Groups[3].Value);
        HappyHours = Tuple.Create(startTime, endTime, timeZone);
        Debug.Print($"Set happy hours from {startTime} to {endTime} in time zone {timeZone.Id}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_settings", "Lists charge damage settings")]
    private static void ListChargeDamageSettings()
    {
        Debug.Print("Charge Damage Settings:");
        Debug.Print($"crpg_charge_damage_disable_all <True/False> | current: {DisableAllChargeDamage}");
        Debug.Print($"crpg_charge_damage_allow_enemies <True/False> | current: {AllowChargeEnemies}");
        Debug.Print($"crpg_charge_damage_allow_friendly <True/False> | current: {AllowFriendlyChargeDamage}");
        Debug.Print($"crpg_charge_damage_mirror_friendly_to_mount <True/False> | current: {MirrorFriendlyChargeDamageMount}");
        Debug.Print($"crpg_charge_damage_mirror_friendly_to_agent <True/False> | current: {MirrorFriendlyChargeDamageAgent}");
        Debug.Print($"crpg_charge_damage_mirror_mount_multiplier <1-100> | current: {MirrorMountDamageMultiplier}");
        Debug.Print($"crpg_charge_damage_mirror_agent_multiplier <1-100> | current: {MirrorAgentDamageMultiplier}");
        Debug.Print($"crpg_charge_damage_mirror_mount_damage_max <0-1000> | current: {MirrorMountDamageMaximum}");
        Debug.Print($"crpg_charge_damage_mirror_mount_damage_min <0-1000> | current: {MirrorMountDamageMinimum}");
        Debug.Print($"crpg_charge_damage_mirror_mount_damage_max_percentage <0-100> | current: {MirrorMountDamageMaximumPercentage}");
        Debug.Print($"crpg_charge_damage_min_velocity_for_friendly_damage <float> | current: {MinimumChargeVelocityForFriendlyDamage}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_disable_all", "Disable all charge damage")]
    private static void SetDisableAllChargeDamage(string? disableAllChargeDamageStr = "False")
    {
        if (string.IsNullOrWhiteSpace(disableAllChargeDamageStr) || disableAllChargeDamageStr == null
            || !bool.TryParse(disableAllChargeDamageStr, out bool disableAllChargeDamage))
        {
            Debug.Print($"Invalid disable all charge damage: {disableAllChargeDamageStr}");
            Debug.Print("Please provide a valid boolean value (true/false).");
            Debug.Print($"Current value: crpg_charge_damage_disable_all {DisableAllChargeDamage}");
            return;
        }

        DisableAllChargeDamage = disableAllChargeDamage;
        Debug.Print($"Set disable all charge damage to {disableAllChargeDamage}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_allow_enemies", "Allow charge damage to enemies")]
    private static void SetAllowChargeEnemies(string? allowChargeEnemiesStr)
    {
        if (string.IsNullOrWhiteSpace(allowChargeEnemiesStr) || allowChargeEnemiesStr == null
            || !bool.TryParse(allowChargeEnemiesStr, out bool allowChargeEnemies))
        {
            Debug.Print($"Invalid allow charge enemies: {allowChargeEnemiesStr}");
            Debug.Print("Please provide a valid boolean value (true/false).");
            Debug.Print($"Current value: crpg_charge_damage_allow_enemies {AllowChargeEnemies}");
            return;
        }

        AllowChargeEnemies = allowChargeEnemies;
        Debug.Print($"Set allow charge enemies to {allowChargeEnemies}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_allow_friendly", "Allow charge damage to friendly agents")]
    private static void SetAllowFriendlyChargeDamage(string? allowFriendlyChargeDamageStr)
    {
        if (string.IsNullOrWhiteSpace(allowFriendlyChargeDamageStr) || allowFriendlyChargeDamageStr == null
            || !bool.TryParse(allowFriendlyChargeDamageStr, out bool allowFriendlyChargeDamage))
        {
            Debug.Print($"Invalid allow friendly charge damage: {allowFriendlyChargeDamageStr}");
            Debug.Print("Please provide a valid boolean value (true/false).");
            Debug.Print($"Current value: crpg_charge_damage_allow_friendly {AllowFriendlyChargeDamage}");
            return;
        }

        AllowFriendlyChargeDamage = allowFriendlyChargeDamage;
        Debug.Print($"Set allow friendly charge damage to {allowFriendlyChargeDamage}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_mirror_friendly_to_mount", "Mirror charge damage to mount for friendly fire")]
    private static void SetMirrorFriendlyChargeDamageMount(string? mirrorFriendlyChargeDamageMountStr)
    {
        if (string.IsNullOrWhiteSpace(mirrorFriendlyChargeDamageMountStr) || (mirrorFriendlyChargeDamageMountStr == null
            || !bool.TryParse(mirrorFriendlyChargeDamageMountStr, out bool mirrorFriendlyChargeDamageMount)))
        {
            Debug.Print($"Invalid mirror friendly charge damage mount: {mirrorFriendlyChargeDamageMountStr}");
            Debug.Print("Please provide a valid boolean value (true/false).");
            Debug.Print($"Current value: crpg_charge_damage_mirror_friendly_to_mount {MirrorFriendlyChargeDamageMount}");
            return;
        }

        MirrorFriendlyChargeDamageMount = mirrorFriendlyChargeDamageMount;
        Debug.Print($"Set mirror friendly charge damage mount to {mirrorFriendlyChargeDamageMount}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_mirror_friendly_to_agent", "Mirror charge damage to agent for friendly fire")]
    private static void SetMirrorFriendlyChargeDamageAgent(string? mirrorFriendlyChargeDamageAgentStr)
    {
        if (string.IsNullOrWhiteSpace(mirrorFriendlyChargeDamageAgentStr) || mirrorFriendlyChargeDamageAgentStr == null
            || !bool.TryParse(mirrorFriendlyChargeDamageAgentStr, out bool mirrorFriendlyChargeDamageAgent))
        {
            Debug.Print($"Invalid mirror friendly charge damage agent: {mirrorFriendlyChargeDamageAgentStr}");
            Debug.Print("Please provide a valid boolean value (true/false).");
            Debug.Print($"Current value: crpg_charge_damage_mirror_friendly_to_agent {MirrorFriendlyChargeDamageAgent}");
            return;
        }

        MirrorFriendlyChargeDamageAgent = mirrorFriendlyChargeDamageAgent;
        Debug.Print($"Set mirror friendly charge damage agent to {mirrorFriendlyChargeDamageAgent}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_mirror_mount_multiplier", "Set the multiplier for charge damage to mount for friendly fire")]
    private static void SetMirrorMountDamageMultiplier(string? mirrorMountDamageMultiplierStr = "")
    {
        if (string.IsNullOrWhiteSpace(mirrorMountDamageMultiplierStr) || mirrorMountDamageMultiplierStr == null
            || !int.TryParse(mirrorMountDamageMultiplierStr, out int mirrorMountDamageMultiplier)
            || mirrorMountDamageMultiplier < 1
            || mirrorMountDamageMultiplier > 100)
        {
            Debug.Print($"Invalid mirror mount damage multiplier: {mirrorMountDamageMultiplierStr}");
            Debug.Print("Please provide a valid integer value between 1 and 100.");
            Debug.Print($"current value: crpg_charge_damage_mirror_mount_multiplier {MirrorMountDamageMultiplier}");
            return;
        }

        MirrorMountDamageMultiplier = mirrorMountDamageMultiplier;
        Debug.Print($"Set mirror mount damage multiplier to {mirrorMountDamageMultiplier}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_mirror_agent_multiplier", "Set the multiplier for charge damage to agent for friendly fire")]
    private static void SetMirrorAgentDamageMultiplier(string? mirrorAgentDamageMultiplierStr)
    {
        if (mirrorAgentDamageMultiplierStr == null || string.IsNullOrWhiteSpace(mirrorAgentDamageMultiplierStr)
            || !int.TryParse(mirrorAgentDamageMultiplierStr, out int mirrorAgentDamageMultiplier)
            || mirrorAgentDamageMultiplier < 1
            || mirrorAgentDamageMultiplier > 100)
        {
            Debug.Print($"Invalid mirror agent damage multiplier: {mirrorAgentDamageMultiplierStr}");
            Debug.Print("Please provide a valid integer value between 1 and 100.");
            Debug.Print($" current value: crpg_charge_damage_mirror_agent_multiplier {MirrorAgentDamageMultiplier}");
            return;
        }

        MirrorAgentDamageMultiplier = mirrorAgentDamageMultiplier;
        Debug.Print($"Set mirror agent damage multiplier to {mirrorAgentDamageMultiplier}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_mirror_mount_damage_max", "Set the maximum damage allowed to the mount for friendly fire")]
    private static void SetFriendlyMountDamageMaximum(string? friendlyMountDamageMaximumStr)
    {
        if (friendlyMountDamageMaximumStr == null || string.IsNullOrWhiteSpace(friendlyMountDamageMaximumStr)
            || !int.TryParse(friendlyMountDamageMaximumStr, out int friendlyMountDamageMaximum)
            || friendlyMountDamageMaximum < 0
            || friendlyMountDamageMaximum > 1000)
        {
            Debug.Print($"Invalid friendly mount damage maximum: {friendlyMountDamageMaximumStr}");
            Debug.Print("Please provide a valid integer value between 0 and 1000.");
            Debug.Print($"Current value: crpg_charge_damage_mirror_mount_damage_max {MirrorMountDamageMaximum}");
            return;
        }

        MirrorMountDamageMaximum = friendlyMountDamageMaximum;
        Debug.Print($"Set friendly mount damage maximum to {friendlyMountDamageMaximum}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_mirror_mount_damage_min", "Set the minimum damage allowed to the mount for friendly fire")]
    private static void SetFriendlyMountDamageMinimum(string? friendlyMountDamageMinimumStr)
    {
        if (friendlyMountDamageMinimumStr == null || string.IsNullOrWhiteSpace(friendlyMountDamageMinimumStr)
            || !int.TryParse(friendlyMountDamageMinimumStr, out int friendlyMountDamageMinimum)
            || friendlyMountDamageMinimum < 0
            || friendlyMountDamageMinimum > 1000)
        {
            Debug.Print($"Invalid friendly mount damage minimum: {friendlyMountDamageMinimumStr}");
            Debug.Print("Please provide a valid integer value between 0 and 1000.");
            Debug.Print($"Current value: crpg_charge_damage_mirror_mount_damage_min {MirrorMountDamageMinimum}");
            return;
        }

        MirrorMountDamageMinimum = friendlyMountDamageMinimum;
        Debug.Print($"Set friendly mount damage minimum to {friendlyMountDamageMinimum}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_mirror_mount_damage_max_percentage", "Set the maximum damage allowed to the mount for friendly fire as a percentage of the mount's health")]
    private static void SetFriendlyMountDamageMaximumPercentage(string? friendlyMountDamageMaximumPercentageStr)
    {
        if (friendlyMountDamageMaximumPercentageStr == null || string.IsNullOrWhiteSpace(friendlyMountDamageMaximumPercentageStr)
            || !int.TryParse(friendlyMountDamageMaximumPercentageStr, out int friendlyMountDamageMaximumPercentage)
            || friendlyMountDamageMaximumPercentage < 0
            || friendlyMountDamageMaximumPercentage > 100)
        {
            Debug.Print($"Invalid friendly mount damage maximum percentage: {friendlyMountDamageMaximumPercentageStr}");
            Debug.Print("Please provide a valid integer value between 0 and 100.");
            Debug.Print($"Current value: crpg_charge_damage_mirror_mount_damage_max_percentage {MirrorMountDamageMaximumPercentage}");
            return;
        }

        MirrorMountDamageMaximumPercentage = friendlyMountDamageMaximumPercentage;
        Debug.Print($"Set friendly mount damage maximum percentage to {friendlyMountDamageMaximumPercentage}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_charge_damage_min_velocity_for_friendly_damage", "Set the minimum charge velocity for friendly damage")]
    private static void SetMinimumChargeVelocityForFriendlyDamage(string? minimumChargeVelocityForFriendlyDamageStr)
    {
        if (minimumChargeVelocityForFriendlyDamageStr == null || string.IsNullOrWhiteSpace(minimumChargeVelocityForFriendlyDamageStr)
            || !float.TryParse(minimumChargeVelocityForFriendlyDamageStr, out float minimumChargeVelocityForFriendlyDamage)
            || minimumChargeVelocityForFriendlyDamage < 0.0f)
        {
            Debug.Print($"Invalid minimum charge velocity for friendly damage: {minimumChargeVelocityForFriendlyDamageStr}");
            Debug.Print("Please provide a valid float value greater than or equal to 0.0.");
            Debug.Print($"Current value: crpg_charge_damage_min_velocity_for_friendly_damage {MinimumChargeVelocityForFriendlyDamage}");
            return;
        }

        MinimumChargeVelocityForFriendlyDamage = minimumChargeVelocityForFriendlyDamage;
        Debug.Print($"Set minimum charge velocity for friendly damage to {minimumChargeVelocityForFriendlyDamage}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_apply_harmony_patches", "Apply Harmony patches")]
    private static void ApplyHarmonyPatches()
    {
        BannerlordPatches.Apply();
    }
}

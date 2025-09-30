using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using Crpg.Module.Common.Models;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.Inventory;

public class UserAndCharacterInfoVM : ViewModel
{
    private string _userName = string.Empty;
    private string _clanName = string.Empty;
    private string _gold = string.Empty;

    private string _characterName = string.Empty;
    private string _characterClassText = string.Empty;
    private string _characterExperienceText = string.Empty;
    private string _characterLevel = string.Empty;
    private string _characterGeneration = string.Empty;

    private string _kills = string.Empty;
    private string _deaths = string.Empty;
    private string _assists = string.Empty;
    private string _playTimeText = string.Empty;

    private static string FormatPlayTime(TimeSpan timeSpan)
    {
        List<string> parts = new();

        if (timeSpan.Days > 0)
        {
            parts.Add($"{timeSpan.Days}d");
        }

        if (timeSpan.Hours > 0)
        {
            parts.Add($"{timeSpan.Hours}h");
        }

        if (timeSpan.Minutes > 0)
        {
            parts.Add($"{timeSpan.Minutes}m");
        }

        // If everything is zero, show "0m"
        if (parts.Count == 0)
        {
            parts.Add("0m");
        }

        return string.Join(" ", parts);
    }

    public UserAndCharacterInfoVM()
    {
        SetDefaults();
        UpdateUserAndCharacterInfo();
    }

    public void UpdateUserAndCharacterInfo()
    {
        InformationManager.DisplayMessage(new InformationMessage("UpdateUserAndCharacterInfo() called"));
        var myPeer = GameNetwork.MyPeer;
        if (myPeer == null)
        {
            SetDefaults();
            return;
        }

        MissionPeer? missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        if (missionPeer == null)
        {
            SetDefaults();
            return;
        }

        var behavior = Mission.Current?.GetMissionBehavior<CrpgCharacterLoadoutBehaviorClient>();
        if (behavior == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgCharacterLoadoutBehaviorClient is required but not found in current mission", Colors.Red));
            return;
        }

        CrpgPeer? crpgPeer = missionPeer.GetComponent<CrpgPeer>();

        var user = crpgPeer?.User;
        var character = user?.Character;
        var stats = behavior.UserCharacterStatistics;

        UserName = behavior.User?.Name ?? "Unknown";
        ClanName = crpgPeer?.Clan?.Name ?? "No Clan";
        Gold = (behavior.User?.Gold ?? 0).ToString("N0");

        CharacterName = behavior.UserCharacter?.Name ?? "Unknown";
        CharacterClassText = character?.Class.ToString() ?? "Unknown";
        CharacterExperienceText = character?.Experience.ToString() ?? "0";
        CharacterLevel = character?.Level.ToString() ?? "0";
        CharacterGeneration = character?.Generation.ToString() ?? "0";

        Kills = stats?.Kills.ToString() ?? "0";
        Deaths = stats?.Deaths.ToString() ?? "0";
        Assists = stats?.Assists.ToString() ?? "0";
        PlayTimeText = FormatPlayTime(stats?.PlayTime ?? TimeSpan.Zero);
    }

    private void SetDefaults()
    {
        UserName = "Unknown";
        ClanName = "No Clan";
        Gold = "0";
        CharacterName = "Unknown";
        CharacterClassText = "Unknown";
        CharacterExperienceText = "0";
        CharacterLevel = "0";
        CharacterGeneration = "0";
        Kills = "0";
        Deaths = "0";
        Assists = "0";
        PlayTimeText = "0m";
    }

    [DataSourceProperty]
    public string CharacterName { get => _characterName; set => SetField(ref _characterName, value, nameof(CharacterName)); }
    [DataSourceProperty]
    public string UserName { get => _userName; set => SetField(ref _userName, value, nameof(UserName)); }
    [DataSourceProperty]
    public string ClanName { get => _clanName; set => SetField(ref _clanName, value, nameof(ClanName)); }
    [DataSourceProperty]
    public string CharacterClassText { get => _characterClassText; set => SetField(ref _characterClassText, value, nameof(CharacterClassText)); }
    [DataSourceProperty]
    public string CharacterExperienceText { get => _characterExperienceText; set => SetField(ref _characterExperienceText, value, nameof(CharacterExperienceText)); }
    [DataSourceProperty]
    public string CharacterLevel { get => _characterLevel; set => SetField(ref _characterLevel, value, nameof(CharacterLevel)); }
    [DataSourceProperty]
    public string CharacterGeneration { get => _characterGeneration; set => SetField(ref _characterGeneration, value, nameof(CharacterGeneration)); }
    [DataSourceProperty]
    public string Gold { get => _gold; set => SetField(ref _gold, value, nameof(Gold)); }
    [DataSourceProperty]
    public string Kills { get => _kills; set => SetField(ref _kills, value, nameof(Kills)); }
    [DataSourceProperty]
    public string Deaths { get => _deaths; set => SetField(ref _deaths, value, nameof(Deaths)); }
    [DataSourceProperty]
    public string Assists { get => _assists; set => SetField(ref _assists, value, nameof(Assists)); }
    [DataSourceProperty]
    public string PlayTimeText { get => _playTimeText; set => SetField(ref _playTimeText, value, nameof(PlayTimeText)); }
}

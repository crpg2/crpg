using System.IO;
using System.Text.Json;
using System.Web;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using static LauncherV3.MainViewModel;

namespace LauncherV3.LauncherHelper;

public class GameInstallationFolderResolver
{
    public record GameInstallationInfo(string InstallationPath, string Program, string? ProgramArguments, string? ProgramWorkingDirectory, Platform Platform);
    public static GameInstallationInfo? CreateGameInstallationInfo(string installationPath, Platform platform)
    {
        if (platform == Platform.Epic)
        {
            return ResolveBannerlordEpicGamesInstallation();
        }

        string? xboxBannerlordExePath = Path.Combine(installationPath, "bin/Gaming.Desktop.x64_Shipping_Client/Launcher.Native.exe");
        string xboxBannerlordExePathBis = Path.Combine(installationPath, "Content/bin/Gaming.Desktop.x64_Shipping_Client/Launcher.Native.exe");
        string xboxBannerlordExePathTertiary = Path.Combine(installationPath, "Mount & Blade II- Bannerlord/Content/bin/Gaming.Desktop.x64_Shipping_Client/Launcher.Native.exe");
        string? steamBannerlordExePath = Path.Combine(installationPath, "bin/Win64_Shipping_Client/Bannerlord.exe");
        if (!File.Exists(xboxBannerlordExePath))
        {
            xboxBannerlordExePath = null;
        }

        if (File.Exists(xboxBannerlordExePathBis))
        {
            xboxBannerlordExePath = xboxBannerlordExePathBis;
        }

        if (File.Exists(xboxBannerlordExePathTertiary))
        {
            xboxBannerlordExePath = xboxBannerlordExePathTertiary;
        }

        string? programPath = platform switch
        {
            Platform.Xbox => xboxBannerlordExePath,
            Platform.Steam => steamBannerlordExePath,
            _ => null,
        };

        if (programPath == null)
        {
            return null;
        }

        return new GameInstallationInfo(
            installationPath,
            programPath,
            platform == Platform.Steam ? "_MODULES_*Native*Multiplayer*cRPG*_MODULES_ /multiplayer" : null,
            platform == Platform.Steam ? Path.GetDirectoryName(programPath) : null,
            platform);
    }

    public static GameInstallationInfo? ResolveBannerlordInstallation()
    {
        var bannerlordInstallation = ResolveBannerlordSteamInstallation();
        if (bannerlordInstallation != null)
        {
            return bannerlordInstallation;
        }

        bannerlordInstallation = ResolveBannerlordEpicGamesInstallation();
        if (bannerlordInstallation != null)
        {
            return bannerlordInstallation;
        }

        bannerlordInstallation = ResolveBannerlordXboxInstallation();
        if (bannerlordInstallation != null)
        {
            return bannerlordInstallation;
        }

        return null;
    }

    public static GameInstallationInfo? ResolveBannerlordSteamInstallation()
    {
        string? steamPath = (string?)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null);
        if (steamPath == null)
        {
            return null;
        }

        string vdfPath = Path.Combine(steamPath, "steamapps/libraryfolders.vdf");
        if (!File.Exists(vdfPath))
        {
            return null;
        }

        var vdf = VdfConvert.Deserialize(File.ReadAllText(vdfPath));

        for (int i = 0; ; i += 1)
        {
            string index = i.ToString();
            if (vdf.Value[index] == null)
            {
                break;
            }

            string? path = vdf.Value[index]?["path"]?.ToString();
            if (path == null)
            {
                continue;
            }

            string bannerlordPath = Path.Combine(path, "steamapps/common/Mount & Blade II Bannerlord");
            string bannerlordExePath = Path.Combine(bannerlordPath, "bin/Win64_Shipping_Client/Bannerlord.exe");
            if (File.Exists(bannerlordExePath))
            {
                return new GameInstallationInfo(
                    bannerlordPath,
                    bannerlordExePath,
                    "_MODULES_*Native*Multiplayer*cRPG*_MODULES_ /multiplayer",
                    Path.GetDirectoryName(bannerlordExePath),
                    Platform.Steam);
            }
        }

        return null;
    }

    public static GameInstallationInfo? ResolveBannerlordEpicGamesInstallation()
    {
        const string appName = "Chickadee";

        string manifestsFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic/EpicGamesLauncher/Data/Manifests");
        if (!Directory.Exists(manifestsFolderPath))
        {
            return null;
        }

        foreach (string manifestPath in Directory.EnumerateFiles(manifestsFolderPath, "*.item"))
        {
            var manifestDoc = JsonSerializer.Deserialize<JsonDocument>(File.ReadAllText(manifestPath));
            if (manifestDoc == null
                || !manifestDoc.RootElement.TryGetProperty("AppName", out var appNameEl)
                || appNameEl.GetString() != appName)
            {
                continue;
            }

            string bannerlordPath = manifestDoc.RootElement.GetProperty("InstallLocation").GetString()!;
            string catalogNamespace = manifestDoc.RootElement.GetProperty("CatalogNamespace").GetString()!;
            string catalogItemId = manifestDoc.RootElement.GetProperty("CatalogItemId").GetString()!;

            string app = $"{catalogNamespace}:{catalogItemId}:{appName}";
            string program = $"com.epicgames.launcher://apps/{HttpUtility.UrlEncode(app)}?action=launch&silent=true";

            string bannerlordExePath = Path.Combine(bannerlordPath, "bin/Win64_Shipping_Client/Bannerlord.exe");
            if (File.Exists(bannerlordExePath))
            {
                return new GameInstallationInfo(bannerlordPath, program, null, null, Platform.Epic);
            }
        }

        return null;
    }

    public static GameInstallationInfo? ResolveBannerlordXboxInstallation()
    {
        // I couldn't find a smart way to find an xbox game so let's just try to find a XboxGames folder in single letter disks.
        for (char disk = 'A'; disk <= 'Z'; disk += (char)1)
        {
            string bannerlordPath = disk + ":/XboxGames/Mount & Blade II- Bannerlord/Content";
            string bannerlordExePath = Path.Combine(bannerlordPath, "bin/Gaming.Desktop.x64_Shipping_Client/Launcher.Native.exe");
            if (File.Exists(bannerlordExePath))
            {
                return new GameInstallationInfo(bannerlordPath, bannerlordExePath, null, null, Platform.Xbox);
            }
        }

        return null;
    }
}

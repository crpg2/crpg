using System.Diagnostics;
using System.Xml.Linq;

namespace Crpg.GameServerManager.Updater;
public static class GameServerUpdater
{
    private const string RemoteVersionUrl = "https://c-rpg.eu/SubModule.xml";
    private static readonly string ServerPath = Environment.GetEnvironmentVariable("mb_server_path")!;
    private static readonly string LocalVersionPath = ServerPath + @"\Modules\cRPG\SubModule.xml";

    public static async Task<bool> RunUpdateAsync()
    {
        try
        {
            Console.WriteLine($"[{DateTime.UtcNow}] Attempting to update gameserver!");
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string exePath = Path.Combine(currentDirectory, "Crpg.Launcher.exe");

            if (!File.Exists(exePath))
            {
                Console.WriteLine($"[{DateTime.UtcNow}] ERROR: Crpg.Launcher.exe not found in: " + exePath);
                return false;
            }

            ProcessStartInfo psi = new()
            {
                FileName = exePath,
                Arguments = $"-server -path \"{ServerPath}\"",
                WorkingDirectory = currentDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            using Process process = new() { StartInfo = psi };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Console.WriteLine(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Console.WriteLine($"[{DateTime.UtcNow}] ERROR: " + e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] ERROR while running Crpg.Launcher: " + ex.Message);
            return false;
        }
    }

    public static async Task<bool> IsLatestVersion()
    {
        try
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(RemoteVersionUrl);
            response.EnsureSuccessStatusCode();

            string remoteXml = await response.Content.ReadAsStringAsync();
            XDocument? remoteDoc = XDocument.Parse(remoteXml);
            string? remoteVersion = remoteDoc?.Root?.Element("Version")?.Attribute("value")?.Value;

            XDocument? localDoc = null;
            string? localVersion = null;
            if (File.Exists(LocalVersionPath))
            {
                localDoc = XDocument.Load(LocalVersionPath);
                localVersion = localDoc?.Root?.Element("Version")?.Attribute("value")?.Value;
            }

            if (remoteVersion == null)
            {
                throw new Exception();
            }

            if (remoteVersion == localVersion)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.UtcNow}] ERROR CHECKING FOR UPDATES! {ex}");
            return true;
        }
    }
}

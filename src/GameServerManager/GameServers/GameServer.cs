using System.Diagnostics;
using System.Text.Json.Serialization;
using Crpg.Domain.Entities.Servers;
using Crpg.GameServerManager.Common;
using Crpg.GameServerManager.Common.Services;

namespace Crpg.GameServerManager.GameServers;
public class GameServer : IGameServer
{
    public GameMode Mode { get; set; }
    public GameModeAlias GameModeAlias => GameModeService.GetInstanceAliasByGameMode(Mode);
    public string Instance => Environment.GetEnvironmentVariable("CRPG_INSTANCE") + GameModeAlias;
    public int Port { get; set; }
    public GameServerSchedule? Schedule { get; set; }
    [JsonIgnore]
    public Process? GameServerProcess { get; private set; }

    public void ExecuteCommand(string command)
    {
        try
        {
            if (!IsRunning())
            {
                Console.WriteLine("Cannot send command. Game server is not running.");
                return;
            }

            if (GameServerProcess!.StandardInput.BaseStream.CanWrite)
            {
                GameServerProcess.StandardInput.AutoFlush = true;
                GameServerProcess.StandardInput.WriteLine(command);
                Console.WriteLine($"[Command Sent] {command}");
            }
            else
            {
                Console.WriteLine("ERROR: Standard input is closed. The server might not accept further commands.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to send command: {ex.Message}");
        }
    }

    public bool IsRunning() => GameServerProcess != null && !GameServerProcess.HasExited;

    public void StopServer()
    {
        if (GameServerProcess == null)
        {
            Console.WriteLine("Cannot find game server process to shut down!");
            return;
        }

        GameServerProcess.Kill();
    }

    public void StartServer()
    {
        string arguments = $"_MODULES_*Native*Multiplayer*cRPG*_MODULES_ /dedicatedcustomserverconfigfile ..\\cRPG\\{GameModeAlias}.txt /DisableErrorReporting /port {Port} /no_watchdog /tickrate 240";
        string exePath = Path.Combine(Environment.GetEnvironmentVariable("mb_server_path") + @"\bin\Win64_Shipping_Server", "DedicatedCustomServer.Starter.exe");

        ProcessStartInfo newProcess = new()
        {
            FileName = exePath,
            Arguments = arguments,
            WorkingDirectory = Environment.GetEnvironmentVariable("mb_server_path") + @"\bin\Win64_Shipping_Server",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = false,
        };

        newProcess.EnvironmentVariables["CRPG_INSTANCE"] = Instance;
        newProcess.EnvironmentVariables["CRPG_SERVICE"] = "crpg-game-server";

        GameServerProcess = Process.Start(newProcess);
    }
}

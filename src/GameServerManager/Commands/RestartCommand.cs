using Crpg.Domain.Entities.Servers;

namespace Crpg.GameServerManager.Commands;
public class RestartCommand : ICommand
{
    public string Name => "restart";
    public string Description => "Restarts the specified game server.";

    public async Task Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: restart <mode>");
            return;
        }

        string modeString = args[0];
        bool isValidMode = Enum.TryParse(modeString, true, out GameMode mode);
        if (!isValidMode)
        {
            Console.Write($"Invalid gamemode {modeString}");
            await Task.CompletedTask;
            return;
        }

        if (GameServers.GameServerManager.Instance.GameServers.TryGetValue(mode, out var server))
        {
            server.StopServer();
            server.StartServer();
            Console.WriteLine($"Restarted {mode}.");
        }
        else
        {
            Console.WriteLine($"Server {mode} not found.");
        }

        await Task.CompletedTask;
    }
}

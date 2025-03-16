using Crpg.Domain.Entities.Servers;

namespace Crpg.GameServerManager.Commands;
public class RunCommand : ICommand
{
    public string Name => "run";
    public string Description => "Executes a specified command against a specified game server.";

    public async Task Execute(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: run <mode> <command>");
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
            server.ExecuteCommand(args[1]);
        }
        else
        {
            Console.WriteLine($"Server {mode} not found.");
        }

        await Task.CompletedTask;
    }
}

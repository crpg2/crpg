namespace Crpg.GameServerManager.Commands;
public class ListCommand : ICommand
{
    public string Name => "list";
    public string Description => "Lists all running game servers.";

    public async Task Execute(string[] args)
    {
        Console.WriteLine("Running Game Servers:");
        foreach (var server in GameServers.GameServerManager.Instance.GameServers)
        {
            Console.WriteLine($"- {server.Key} (Port: {server.Value.Port}, Running: {server.Value.IsRunning()})");
        }

        await Task.CompletedTask;
    }
}

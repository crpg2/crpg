using Crpg.GameServerManager.Commands;
using Crpg.GameServerManager.GameServers;

GameServerManager manager = GameServerManager.Instance;
manager.RegisterShutdownHandler();
manager.InitialiseGameServers();
CommandProcessor cliProcessor = new();

using var cts = new CancellationTokenSource();
Task runManagerTask = manager.RunAsync(cts.Token);
Task runCliTask = cliProcessor.StartAsync(cts.Token);

await Task.WhenAll(runManagerTask, runCliTask);

Console.WriteLine("Shutting down Game Server Manager...");
manager.StopAllServers();

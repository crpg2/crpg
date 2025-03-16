namespace Crpg.GameServerManager.Commands;
public class CommandProcessor
{
    private readonly CommandHandler _commandHandler;

    public CommandProcessor()
    {
        _commandHandler = new CommandHandler();
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Game Server Manager - CLI Mode");
        Console.WriteLine("Type 'help' for commands, or 'exit' to quit.");

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.Write("\n> ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                _commandHandler.ShowAvailableCommands();
                continue;
            }

            await _commandHandler.ExecuteCommand(input);
        }
    }
}

namespace Crpg.GameServerManager.Commands;
public class CommandHandler
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public CommandHandler()
    {
        RegisterCommand(new ListCommand());
        RegisterCommand(new RestartCommand());
        RegisterCommand(new RunCommand());
    }

    public async Task ExecuteCommand(string input)
    {
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        string commandName = parts[0].ToLower();
        string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        if (_commands.TryGetValue(commandName, out ICommand? command))
        {
            await command.Execute(args);
        }
        else
        {
            Console.WriteLine($"Unknown command: {commandName}. Type 'help' for available commands.");
        }
    }

    public void ShowAvailableCommands()
    {
        Console.WriteLine("Available commands:");
        foreach (var command in _commands.Values)
        {
            Console.WriteLine($"  {command.Name} - {command.Description}");
        }
    }

    private void RegisterCommand(ICommand command)
    {
        _commands[command.Name.ToLower()] = command;
    }
}

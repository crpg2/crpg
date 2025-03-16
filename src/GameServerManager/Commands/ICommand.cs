namespace Crpg.GameServerManager.Commands;
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    Task Execute(string[] args);
}

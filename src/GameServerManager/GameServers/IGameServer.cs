namespace Crpg.GameServerManager.GameServers;
internal interface IGameServer
{
    void StartServer();

    void StopServer();

    bool IsRunning();

    void ExecuteCommand(string command);
}

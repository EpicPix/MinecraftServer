namespace MinecraftServer;

public class Server
{
    public ServerInfo ServerInfo { get; }

    public Server()
    {
        ServerInfo = new ServerInfo {
            description = new ServerInfo.DescriptionInfo {
                text = DateTime.UtcNow.ToString()
            }
        };
    }
}
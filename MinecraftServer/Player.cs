using MinecraftServer.Networking;

namespace MinecraftServer;

public class Player
{

    public uint EntityId { get; }
    public Guid Uuid { get; }
    
    public NetworkConnection Connection { get; }

    private int _ping;
    public int Ping {
        get => _ping;
        set {
            Console.WriteLine($"Ping for {Username} set to {value}ms");
            _ping = value;
        }
    }
    
    public string Username { get; }
    
    public double X = 0;
    public double Y = 0;
    public double Z = 0;

    public Player(NetworkConnection connection, uint entityId)
    {
        Connection = connection;
        Username = connection.Username;
        Uuid = connection.Uuid;
        EntityId = entityId;
        Console.WriteLine(entityId);
    }
    
}
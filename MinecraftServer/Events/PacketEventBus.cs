using MinecraftServer.Networking;
using MinecraftServer.SourceGenerators.Events;

namespace MinecraftServer.Events;

[EventBus(EventBuses.Packet)]
public partial class PacketEventBus : CancellableEventBus
{
    public NetworkConnection Connection { get; }
    public Server Server { get; }

    public PacketEventBus(NetworkConnection connection, Server server)
    {
        Connection = connection;
        Server = server;
    }
}
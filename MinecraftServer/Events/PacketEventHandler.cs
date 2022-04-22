using MinecraftServer.Networking;
using MinecraftServer.Packets;

namespace MinecraftServer.Events;

public abstract class PacketEventHandler
{
    public Packet Packet { get; }

    public PacketEventHandler(Packet packet)
    {
        Packet = packet;
    }

    public abstract void Run(PacketData data, NetworkConnection connection, Server server);
}

public class PacketEventHandler<T> : PacketEventHandler where T : PacketData
{

    private Action<T, NetworkConnection, Server> _action;

    public PacketEventHandler(Packet packet, Action<T, NetworkConnection, Server> action) : base(packet)
    {
        _action = action;
    }

    public override void Run(PacketData data, NetworkConnection connection, Server server)
    {
        _action.Invoke((T) data, connection, server);
    }
}
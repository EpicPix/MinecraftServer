using MinecraftServer.Networking;
using MinecraftServer.Packets;

namespace MinecraftServer.Events;

public abstract class PacketEventHandler
{
    public Packet Packet { get; }
    public long Priority { get; }

    public PacketEventHandler(Packet packet, long priority)
    {
        Packet = packet;
        Priority = priority;
    }

    public abstract PacketEventHandlerStatus Run(PacketData data, NetworkConnection connection, Server server);
}

public class PacketEventHandler<T> : PacketEventHandler where T : PacketData
{

    private Func<T, NetworkConnection, Server, PacketEventHandlerStatus> _func;

    public PacketEventHandler(Packet packet, long priority, Func<T, NetworkConnection, Server, PacketEventHandlerStatus> func) : base(packet, priority)
    {
        _func = func;
    }

    public override PacketEventHandlerStatus Run(PacketData data, NetworkConnection connection, Server server)
    {
        return _func.Invoke((T) data, connection, server);
    }
}

[Flags]
public enum PacketEventHandlerStatus
{
    Continue = 0x01, // continue running events
    Stop = 0x02, // stop running any events after
}
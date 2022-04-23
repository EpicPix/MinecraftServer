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

    public abstract void Run(PacketData data, NetworkConnection connection, Server server, ref PacketEventHandlerStatus status);
}

public class PacketEventHandler<T> : PacketEventHandler where T : PacketData
{

    private PacketEventHandlerFuncStatus _func;
    
    public delegate void PacketEventHandlerFunc(T packetData, NetworkConnection connection, Server server);
    public delegate void PacketEventHandlerFuncStatus(T packetData, NetworkConnection connection, Server server, ref PacketEventHandlerStatus status);

    public PacketEventHandler(Packet packet, long priority, PacketEventHandlerFunc func)
        : this(packet, priority, (T packetData, NetworkConnection connection, Server server, ref PacketEventHandlerStatus status) => func.Invoke(packetData, connection, server))
    { }
    
    public PacketEventHandler(Packet packet, long priority, PacketEventHandlerFuncStatus func) : base(packet, priority)
    {
        _func = func;
    }

    public override void Run(PacketData data, NetworkConnection connection, Server server, ref PacketEventHandlerStatus status)
    {
        _func.Invoke((T) data, connection, server, ref status);
    }
}

[Flags]
public enum PacketEventHandlerStatus
{
    Continue = 0x01, // continue running events
    Stop = 0x02, // stop running any events after
}
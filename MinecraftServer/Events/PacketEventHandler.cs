using MinecraftServer.Networking;
using MinecraftServer.Packets;

namespace MinecraftServer.Events;

public abstract class PacketEventHandler
{
    public Packet Packet { get; }
    public long Priority { get; }
    public bool Async { get; }

    public PacketEventHandler(Packet packet, long priority, bool async)
    {
        Packet = packet;
        Priority = priority;
        Async = async;
    }

    public abstract void Run(PacketData data, NetworkConnection connection, Server server, ref PacketEventHandlerStatus status);

    public abstract ValueTask RunAsync(PacketData data, NetworkConnection connection, Server server, PacketEventHandlerStatusRef status);
}

public class PacketEventHandler<T> : PacketEventHandler where T : PacketData
{

    private PacketEventHandlerFuncStatus _func = null!;
    private PacketEventHandlerFuncStatusAsync _asyncFunc = null!;
    
    public delegate void PacketEventHandlerFunc(T packetData, NetworkConnection connection, Server server);
    public delegate void PacketEventHandlerFuncStatus(T packetData, NetworkConnection connection, Server server, ref PacketEventHandlerStatus status);
    public delegate ValueTask PacketEventHandlerFuncAsync(T packetData, NetworkConnection connection, Server server);
    public delegate ValueTask PacketEventHandlerFuncStatusAsync(T packetData, NetworkConnection connection, Server server, PacketEventHandlerStatusRef status);

    public PacketEventHandler(Packet packet, long priority, PacketEventHandlerFunc func)
        : this(packet, priority, (T packetData, NetworkConnection connection, Server server, ref PacketEventHandlerStatus status) => func.Invoke(packetData, connection, server))
    { }
    
    public PacketEventHandler(Packet packet, long priority, PacketEventHandlerFuncStatus func) : base(packet, priority, false)
    {
        _func = func;
    }
    
    public PacketEventHandler(Packet packet, long priority, PacketEventHandlerFuncAsync func)
        : this(packet, priority, (packetData, connection, server, status) => func.Invoke(packetData, connection, server))
    { }
    
    public PacketEventHandler(Packet packet, long priority, PacketEventHandlerFuncStatusAsync func) : base(packet, priority, true)
    {
        _asyncFunc = func;
    }

    public override void Run(PacketData data, NetworkConnection connection, Server server, ref PacketEventHandlerStatus status)
    {
        _func.Invoke((T) data, connection, server, ref status);
    }

    public override async ValueTask RunAsync(PacketData data, NetworkConnection connection, Server server, PacketEventHandlerStatusRef status)
    {
        await _asyncFunc.Invoke((T) data, connection, server, status);
    }
}

[Flags]
public enum PacketEventHandlerStatus
{
    Continue = 0x01, // continue running events
    Stop = 0x02, // stop running any events after
}

public class PacketEventHandlerStatusRef
{
    public PacketEventHandlerStatus HandlerStatus;

    public PacketEventHandlerStatusRef(PacketEventHandlerStatus handlerStatus)
    {
        HandlerStatus = handlerStatus;
    }
}
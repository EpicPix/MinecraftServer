using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using MinecraftServer.Networking;

namespace MinecraftServer.Packets;

public abstract class Packet
{
    public abstract PacketType Type { get; }
    public abstract PacketBound Bound { get; }
    public abstract uint Id { get; }

    private static List<Packet> _packets = new ();

    static Packet() {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type != typeof(Packet) && type != typeof(Packet<,>) && typeof(Packet).IsAssignableFrom(type))
            {
                var inst = Activator.CreateInstance(type);
                if (inst != null)
                {
                    _packets.Add((Packet) inst);
                }
            }
        }
    }
    public static bool TryGetPacket(PacketType type, PacketBound bound, uint id, [NotNullWhen(true)] out Packet? result)
    {
        foreach (var packet in _packets)
        {
            if (packet.Type == type && packet.Bound == bound && packet.Id == id)
            {
                result = packet;
                return true;
            }
        }
        result = null;
        return false;
    }

    public static T GetPacket<T>() where T : Packet
    {
        foreach (var packet in _packets)
        {
            if (packet.GetType() == typeof(T))
            {
                return (T) packet;
            }
        }

        throw new ArgumentException($"Unknown packet of type {typeof(T)}");
    }

    public static Packet GetPacket(Type type)
    {
        foreach (var packet in _packets)
        {
            if (packet.GetType() == type)
            {
                return packet;
            }
        }

        throw new ArgumentException($"Unknown packet of type {type}");
    }
    
    public abstract ValueTask<PacketData> ReadPacket(DataAdapter stream);
    
    public abstract ValueTask WritePacket(DataAdapter stream, PacketData data);
    public async ValueTask SendPacket(PacketData data, NetworkConnection connection, Func<NetworkConnection, ValueTask> runOnCompletion, bool priority = false)
    {
        try
        {
            var pkt = new PlayerPacketQueue.QueuedPacket(this, data, runOnCompletion);
            if(priority) connection.PacketQueue.SetPriorityPacket(pkt.PacketCountId);
            await connection.PacketQueue.Queue.Writer.WriteAsync(pkt);
        }
        catch (ChannelClosedException) { }
    }
    public async ValueTask SendPacket(PacketData data, NetworkConnection connection, bool priority = false)
    {
        try
        {
            var pkt = new PlayerPacketQueue.QueuedPacket(this, data, _ => ValueTask.CompletedTask);
            if(priority) connection.PacketQueue.SetPriorityPacket(pkt.PacketCountId);
            await connection.PacketQueue.Queue.Writer.WriteAsync(pkt);
        }
        catch (ChannelClosedException) { }
    }
}

public abstract class Packet<TPacket, TPacketData> : Packet where TPacket : Packet<TPacket, TPacketData> where TPacketData : PacketData
{
    public static async ValueTask SendAsync(TPacketData data, NetworkConnection connection, bool priority = false)
    {
        var packet = GetPacket<TPacket>();
        await packet.SendPacket(data, connection, priority);
    }
    
    public static void Send(TPacketData data, NetworkConnection connection, bool priority = false)
    {
        SendAsync(data, connection, priority).GetAwaiter().GetResult();
    }
    
    public static async ValueTask SendAsync(TPacketData data, NetworkConnection connection, Func<NetworkConnection, ValueTask> runOnCompletion, bool priority = false)
    {
        var packet = GetPacket<TPacket>();
        await packet.SendPacket(data, connection, runOnCompletion, priority);
    }
    
    public static void Send(TPacketData data, NetworkConnection connection, Func<NetworkConnection, ValueTask> runOnCompletion, bool priority = false)
    {
        var packet = GetPacket<TPacket>();
        packet.SendPacket(data, connection, runOnCompletion, priority).GetAwaiter().GetResult();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TPacketData Of(PacketData data){
        return (TPacketData) data;
    }
}

public enum PacketBound
{
    Server, Client
}

public enum PacketType
{
    Handshake = 0,
    Status = 1,
    Login = 2,
    Play = 3
}
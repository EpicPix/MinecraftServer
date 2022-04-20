using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.IO;
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

    public static Packet GetPacket(PacketType type, PacketBound bound, uint id)
    {
        foreach (var packet in _packets)
        {
            if (packet.Type == type && packet.Bound == bound && packet.Id == id)
            {
                return packet;
            }
        }

        throw new ArgumentException($"Unknown packet {bound}bound {type} with id {id}");
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
    
    public abstract ValueTask<PacketData> ReadPacket(DataAdapter stream);
    
    public abstract ValueTask WritePacket(DataAdapter stream, PacketData data);
    public async ValueTask SendPacket(PacketData data, NetworkConnection connection, Func<ValueTask> runOnCompletion)
    {
        await connection.PacketQueue.Queue.Writer.WriteAsync(new PlayerPacketQueue.QueuedPacket(this, data, runOnCompletion));
    }
    public async ValueTask SendPacket(PacketData data, NetworkConnection connection)
    {
        await connection.PacketQueue.Queue.Writer.WriteAsync(new PlayerPacketQueue.QueuedPacket(this, data, ()=> ValueTask.CompletedTask));
    }
}

public abstract class Packet<TPacket, TPacketData> : Packet where TPacket : Packet<TPacket, TPacketData> where TPacketData : PacketData
{
    public static async ValueTask Send(TPacketData data, NetworkConnection connection)
    {
        var packet = GetPacket<TPacket>();
        await packet.SendPacket(data, connection);
    }
    
    public static async ValueTask Send(TPacketData data, NetworkConnection connection, Func<ValueTask> runOnCompletion)
    {
        var packet = GetPacket<TPacket>();
        await packet.SendPacket(data, connection, runOnCompletion);
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
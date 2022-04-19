using System.Reflection;
using System.Runtime.CompilerServices;

namespace MinecraftServer.Packets;

public abstract class Packet
{
    public abstract PacketType Type { get; }
    public abstract PacketSide Side { get; }
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

    public static Packet GetPacket(PacketType type, PacketSide side, uint id)
    {
        foreach (var packet in _packets)
        {
            if (packet.Type == type && packet.Side == side && packet.Id == id)
            {
                return packet;
            }
        }

        throw new ArgumentException($"Unknown packet {type} at {side} with id {id}");
    }

    // public static Packet GetPacket(Type t)
    // {
    //     foreach (var packet in _packets)
    //     {
    //         if (packet.GetType() == t)
    //         {
    //             return packet;
    //         }
    //     }

    //     throw new ArgumentException($"Unknown packet of type {t}");
    // }

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
    
    public abstract Task<PacketData> ReadPacket(NetworkConnection stream);
    
    public abstract Task WritePacket(NetworkConnection stream, PacketData data);
    public async ValueTask SendPacket(PacketData data, NetworkConnection connection)
    {
        using var stream = new MemoryStream();
        await WritePacket(new NetworkConnection(stream), data);
        await connection.WriteVarInt((int)stream.Length + 1);
        await connection.WriteVarInt((int)Id);
        await connection.WriteBytes(stream.GetBuffer());
        await connection.Flush();
    }
}

public abstract class Packet<R, T> : Packet where T : Packet<R, T> where R : PacketData
{
    public static ValueTask Send(R data, NetworkConnection connection)
    {
        var packet = GetPacket<T>();
        return packet.SendPacket(data, connection);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static R Of(PacketData data){
        return (R)data;
    }
}

public enum PacketSide
{
    Client, Server
}

public enum PacketType
{
    Handshake = 0,
    Status = 1,
    Login = 2,
    Play = 3
}
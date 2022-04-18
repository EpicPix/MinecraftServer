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
    
}

public abstract class Packet<T, R> : Packet, ISendable<T> where R : Packet<T, R> where T : PacketData
{
    public abstract Task<T> ReadPacket(NetworkConnection stream);
    public abstract Task WritePacket(NetworkConnection stream, T data);

    public static async ValueTask SendPacket(T data, NetworkConnection connection)
    {
        using var stream = new MemoryStream();
        var packet = GetPacket<R>();
        await packet.WritePacket(new NetworkConnection(stream), data);
        await connection.WriteVarInt((int)stream.Length + 1);
        await connection.WriteVarInt((int)packet.Id);
        await connection.WriteBytes(stream.GetBuffer());
        await connection.Flush();
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
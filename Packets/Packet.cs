using System.Reflection;

namespace MinecraftServer.Packets;

public interface Packet
{
    public PacketType Type { get; }
    public PacketSide Side { get; }
    public uint Id { get; }

    private static List<Packet>? _packets;
    
    internal static IEnumerable<Type> GetAvailablePackets(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type != typeof(Packet) && type != typeof(Packet<>) && typeof(Packet).IsAssignableFrom(type))
            {
                yield return type;
            }
        }
    }
    
    internal static void LoadPackets()
    {
        _packets = new List<Packet>();

        foreach (var type in GetAvailablePackets(Assembly.GetExecutingAssembly()))
        {
            var inst = Activator.CreateInstance(type);
            if (inst != null)
            {
                _packets.Add((Packet) inst);
            }
        }
    }
    
    static Packet GetPacket(PacketType type, PacketSide side, uint id)
    {
        if(_packets == null) LoadPackets();
        if (_packets == null) throw new NullReferenceException("Packets are null");
        foreach (var packet in _packets)
        {
            if (packet.Type == type && packet.Side == side && packet.Id == id)
            {
                return packet;
            }
        }

        throw new ArgumentException($"Unknown packet {type} at {side} with id {id}");
    }

    static T GetPacket<T>() where T : Packet
    {
        if(_packets == null) LoadPackets();
        if (_packets == null) throw new NullReferenceException("Packets are null");
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

public interface Packet<T> : Packet where T : PacketData
{
    public Task<T> ReadPacket(NetworkConnection stream);
    public Task WritePacket(NetworkConnection stream, T data);

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

public class PacketData {}
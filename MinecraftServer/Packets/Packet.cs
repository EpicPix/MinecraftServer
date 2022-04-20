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
    public async ValueTask SendPacket(PacketData data, NetworkConnection connection)
    {
        if (connection.IsCompressed)
        {
            await using var stream = Server.MS_MANAGER.GetStream();
            // get packet data
            await WritePacket(new StreamAdapter(stream), data);
            stream.TryGetBuffer(out var packetData);
            var idLength = Utils.GetVarIntLength((int) Id);
            if (packetData.Count + idLength + Utils.GetVarIntLength(0) < Server.NetworkCompressionThreshold)
            {
                await connection.WriteVarInt(packetData.Count + idLength + Utils.GetVarIntLength(0));
                await connection.WriteVarInt(0);
                await connection.WriteVarInt((int)Id);
                await connection.WriteBytes(packetData);
                return;
            }
            
            await using var compressedStream = Server.MS_MANAGER.GetStream();
            var comprAdapter = new CompressionAdapter(new StreamAdapter(compressedStream));
            
            // write id
            await comprAdapter.WriteVarInt((int)Id);
            // write data
            await comprAdapter.WriteBytes(packetData);
        
            comprAdapter.Flush();
            
            compressedStream.TryGetBuffer(out var compressedPacketData);
            
            var uncompressedPacketSize = packetData.Count + idLength;
            var compressedPacketSize = compressedPacketData.Count + Utils.GetVarIntLength(uncompressedPacketSize);
            
            await connection.WriteVarInt(compressedPacketSize);
            await connection.WriteVarInt(uncompressedPacketSize);
            await connection.WriteBytes(compressedPacketData);
        }
        else
        {
            await using var stream = Server.MS_MANAGER.GetStream();
            await WritePacket(new StreamAdapter(stream), data);
            await connection.WriteVarInt((int)stream.Length + Utils.GetVarIntLength((int) Id));
            await connection.WriteVarInt((int)Id);
            stream.TryGetBuffer(out var buf);
            await connection.WriteBytes(buf);
        }
    }
}

public abstract class Packet<TPacket, TPacketData> : Packet where TPacket : Packet<TPacket, TPacketData> where TPacketData : PacketData
{
    public static async ValueTask Send(TPacketData data, NetworkConnection connection)
    {
        var packet = GetPacket<TPacket>();
        await packet.SendPacket(data, connection);
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
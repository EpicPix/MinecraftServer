using System.Threading.Channels;
using MinecraftServer.Packets;

namespace MinecraftServer.Networking;

public class PlayerPacketQueue
{
    public Channel<QueuedPacket> Queue { get; } = Channel.CreateBounded<QueuedPacket>(64);
    public record struct QueuedPacket(Packet PacketDefinition, PacketData PacketData, Func<NetworkConnection, ValueTask> Completion);

    public async ValueTask ForwardPackets(NetworkConnection connection)
    {
        await foreach (var packet in Queue.Reader.ReadAllAsync())
        {
            Console.WriteLine($"[S->C] {packet.PacketDefinition}");
            try
            {
                var data = packet.PacketData;
                var id = packet.PacketDefinition.Id;
                await using var stream = Server.MS_MANAGER.GetStream();
                await packet.PacketDefinition.WritePacket(new StreamAdapter(stream), data);
                stream.TryGetBuffer(out var packetData);
                if (connection.IsCompressed)
                {
                    var idLength = Utils.GetVarIntLength((int)id);
                    if (packetData.Count + idLength + Utils.GetVarIntLength(0) < Server.NetworkCompressionThreshold)
                    {
                        await connection.WriteVarInt(packetData.Count + idLength + Utils.GetVarIntLength(0));
                        await connection.WriteVarInt(0);
                        await connection.WriteVarInt((int)id);
                        await connection.WriteBytes(packetData);
                        continue;
                    }

                    await using var compressedStream = Server.MS_MANAGER.GetStream();
                    var comprAdapter = new CompressionAdapter(new StreamAdapter(compressedStream));

                    // write id
                    await comprAdapter.WriteVarInt((int)id);
                    // write data
                    await comprAdapter.WriteBytes(packetData);

                    comprAdapter.Flush();

                    compressedStream.TryGetBuffer(out var compressedPacketData);

                    var uncompressedPacketSize = packetData.Count + idLength;
                    var compressedPacketSize =
                        compressedPacketData.Count + Utils.GetVarIntLength(uncompressedPacketSize);

                    await connection.WriteVarInt(compressedPacketSize);
                    await connection.WriteVarInt(uncompressedPacketSize);
                    await connection.WriteBytes(compressedPacketData);
                }
                else
                {
                    await connection.WriteVarInt((int)stream.Length + Utils.GetVarIntLength((int)id));
                    await connection.WriteVarInt((int)id);
                    await connection.WriteBytes(packetData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed write packet {packet.PacketDefinition}. {e.Message} {e.StackTrace}");
            }

            try
            {
                await packet.Completion(connection);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Failed to execute post packet-sent code. {e.Message} {e.StackTrace}");
            }
        }
        Console.WriteLine("finished");
    }
}
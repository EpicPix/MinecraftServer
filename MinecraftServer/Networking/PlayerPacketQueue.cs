using System.Threading.Channels;
using MinecraftServer.Events;
using MinecraftServer.Packets;

namespace MinecraftServer.Networking;

public class PlayerPacketQueue
{

    public Server Server { get; }

    public Channel<QueuedPacket> Queue { get; } = Channel.CreateBounded<QueuedPacket>(64);

    public record struct QueuedPacket(Packet PacketDefinition, PacketData PacketData,
        Func<NetworkConnection, ValueTask> Completion)
    {
        private static volatile uint _internalPacketCount = 0;
        public readonly uint PacketCountId = Interlocked.Increment(ref _internalPacketCount);
    }

    private uint _skipUntil = 0;

    public PlayerPacketQueue(Server server)
    {
        Server = server;
    }
    
    public void SetPriorityPacket(uint id)
    {
        _skipUntil = id;
    }

    public async ValueTask ForwardPackets(NetworkConnection connection)
    {
        try
        {
            await foreach (var packet in Queue.Reader.ReadAllAsync())
            {
                if (packet.PacketCountId < _skipUntil) continue;
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
                        } else
                        {

                            await using var compressedStream = Server.MS_MANAGER.GetStream();
                            var comprAdapter = new CompressionAdapter(new StreamAdapter(compressedStream), connection.ConnectionState);

                            // write id
                            await comprAdapter.WriteVarInt((int) id);
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
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to execute post packet-sent code. {e.Message} {e.StackTrace}");
                }

                if (Server != null) // might be null, the only place it is are the benchmarks
                {
                    foreach (var packetHandler in Server.PacketHandlers)
                    {
                        if (packetHandler.Packet == packet.PacketDefinition)
                        {
                            var status = packetHandler.Run(packet.PacketData, connection, Server);
                            if ((status & PacketEventHandlerStatus.Stop) == PacketEventHandlerStatus.Stop)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            await connection.Disconnect("", true);
            Console.WriteLine("finished");
        }
    }

    public void Stop()
    {
        Queue.Writer.Complete();
    }
}
using System.Threading.Channels;
using MinecraftServer.Events;
using MinecraftServer.Packets;

namespace MinecraftServer.Networking;

public class PlayerPacketQueue
{

    public Server Server { get; }

    public Channel<QueuedPacket> Queue { get; } = Channel.CreateUnbounded<QueuedPacket>();

    public record struct QueuedPacket(Packet PacketDefinition, PacketData PacketData,
        Func<NetworkConnection, ValueTask> Completion)
    {
        private static volatile uint _internalPacketCount = 0;
        public readonly uint PacketCountId = Interlocked.Increment(ref _internalPacketCount);
    }

    private uint _skipUntil = 0;
    private PacketEventBus _bus;

    public PlayerPacketQueue(Server server, PacketEventBus bus)
    {
        Server = server;
        _bus = bus;
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
                            await connection.WriteVarIntAsync(packetData.Count + idLength + Utils.GetVarIntLength(0));
                            await connection.WriteVarIntAsync(0);
                            await connection.WriteVarIntAsync((int)id);
                            await connection.WriteBytesAsync(packetData);
                        }
                        else
                        {

                            await using var compressedStream = Server.MS_MANAGER.GetStream();
                            var comprAdapter = new CompressionAdapter(new StreamAdapter(compressedStream),
                                connection.ConnectionState);

                            // write id
                            await comprAdapter.WriteVarIntAsync((int)id);
                            // write data
                            await comprAdapter.WriteBytesAsync(packetData);

                            comprAdapter.Flush();

                            compressedStream.TryGetBuffer(out var compressedPacketData);

                            var uncompressedPacketSize = packetData.Count + idLength;
                            var compressedPacketSize =
                                compressedPacketData.Count + Utils.GetVarIntLength(uncompressedPacketSize);

                            await connection.WriteVarIntAsync(compressedPacketSize);
                            await connection.WriteVarIntAsync(uncompressedPacketSize);
                            await connection.WriteBytesAsync(compressedPacketData);
                        }
                    }
                    else
                    {
                        await connection.WriteVarIntAsync((int)stream.Length + Utils.GetVarIntLength((int)id));
                        await connection.WriteVarIntAsync((int)id);
                        await connection.WriteBytesAsync(packetData);
                    }
                }
                catch (IOException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed write packet {packet.PacketDefinition}. {e.Message} {e.StackTrace}");
                    break;
                }

                try
                {
                    await packet.Completion(connection);
                } catch (Exception e)
                {
                    Console.WriteLine($"Failed to execute post packet-sent code. {e.Message} {e.StackTrace}");
                }

                if (Server != null) // might be null, the only place it is are the benchmarks
                {
                    await PacketEventBus.PostEventAsync(packet.PacketData, _bus);
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
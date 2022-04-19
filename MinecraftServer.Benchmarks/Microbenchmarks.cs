using BenchmarkDotNet.Attributes;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Serverbound;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Benchmarks;

[MemoryDiagnoser]
public class Microbenchmarks
{
    private static readonly Packet TestPacket = Packet.GetPacket<CsHandshake>();
    private static readonly CsHandshakePacketData TestPacketData = new (9999, "testing-server-ip", UInt16.MaxValue, 1);
    private static readonly NetworkConnection NullOutput = new(Stream.Null);
    
    [Benchmark]
    public Packet GenericGetPacketPerformance()
    {
        return Packet.GetPacket<CsHandshake>();
    }
    
    [Benchmark]
    public Packet DynamicGetPacketPerformance()
    {
        return Packet.GetPacket(TestPacket.Type, TestPacket.Side, TestPacket.Id);
    }
    
    [Benchmark]
    public async ValueTask SerializePacketPerformance()
    {
        await TestPacket.SendPacket(TestPacketData, NullOutput);
    }
}
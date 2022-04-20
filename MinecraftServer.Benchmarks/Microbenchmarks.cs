using BenchmarkDotNet.Attributes;
using MinecraftServer.Networking;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Serverbound;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Handshake;

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
    public bool DynamicGetPacketPerformance()
    {
        return Packet.TryGetPacket(TestPacket.Type, TestPacket.Bound, TestPacket.Id, out _);
    }
    
    [Benchmark]
    public ValueTask SerializePacketPerformance()
    {
        return TestPacket.SendPacket(TestPacketData, NullOutput);
    }
}
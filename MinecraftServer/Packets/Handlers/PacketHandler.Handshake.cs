using MinecraftServer.Events;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Handshake;
using MinecraftServer.SourceGenerators.Events;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{

    // [PacketEvent(typeof(CsHandshake), priority: 100)]
    [EventHandler(1, typeof(CsHandshakePacketData), 100)]
    public static void HandleHandshake(CsHandshakePacketData data, PacketEventBus bus)
    {
        Console.WriteLine($"Protocol version: {data.ProtocolVersion} / Server IP: {data.ServerIp} / Server Port: {data.ServerPort} / Next State: {(PacketType) data.NextState}");
        if (data.NextState == (int) PacketType.Status)
        {
            bus.Connection.ChangeState(PacketType.Status);
        }else if (data.NextState == (int) PacketType.Login)
        {
            bus.Connection.ChangeState(PacketType.Login);
        } else
        {
            throw new NotSupportedException($"Unsupported next state {data.NextState}");
        }
    }
    
}
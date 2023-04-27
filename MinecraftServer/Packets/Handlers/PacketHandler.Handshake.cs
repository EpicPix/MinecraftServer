using MinecraftServer.Events;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.SourceGenerators.Events;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{

    [EventHandler(EventBuses.Packet, typeof(CsHandshakePacketData), priority: 100, true)]
    public static async ValueTask HandleHandshake(CsHandshakePacketData data, PacketEventBus bus)
    {
        Console.WriteLine($"Protocol version: {data.ProtocolVersion} / Server IP: {data.ServerIp} / Server Port: {data.ServerPort} / Next State: {(PacketType) data.NextState}");
        if (data.NextState == (int) PacketType.Status)
        {
            bus.Connection.ChangeState(PacketType.Status);
        }else if (data.NextState == (int) PacketType.Login)
        {
            if(data.ProtocolVersion != 462)
            {
                await bus.Connection.Disconnect($"Invalid protocol version. Expected 462, got {data.ProtocolVersion}");
                return;
            }
            bus.Connection.ChangeState(PacketType.Login);
        } else
        {
            throw new NotSupportedException($"Unsupported next state {data.NextState}");
        }
    }
    
}
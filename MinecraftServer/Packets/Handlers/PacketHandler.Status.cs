using MinecraftServer.Events;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Status;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Status;
using MinecraftServer.SourceGenerators.Events;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{

    // [PacketEvent(typeof(CsStatusRequest), priority: 100)]
    [EventHandler(1, typeof(CsStatusRequestPacketData), 100)]
    public static void HandleStatusRequest(CsStatusRequestPacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        ScStatusResponse.Send(new (server.ServerInfo), connection);
    }

    // [PacketEvent(typeof(CsStatusPing), priority: 100)]
    [EventHandler(1, typeof(CsStatusPingPacketData), 100)]
    public static void HandleStatusPing(CsStatusPingPacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        ScStatusPong.Send(data, connection);
    }

}
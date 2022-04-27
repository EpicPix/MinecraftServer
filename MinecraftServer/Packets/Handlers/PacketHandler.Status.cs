using MinecraftServer.Events;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Status;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Status;
using MinecraftServer.SourceGenerators.Events;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{

    [EventHandler(EventBuses.Packet, typeof(CsStatusRequestPacketData), priority: 100)]
    public static void HandleStatusRequest(CsStatusRequestPacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        ScStatusResponse.Send(new (server.ServerInfo), connection);
    }

    [EventHandler(EventBuses.Packet, typeof(CsStatusPingPacketData), priority: 100)]
    public static void HandleStatusPing(CsStatusPingPacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        ScStatusPong.Send(data, connection);
    }

}
using MinecraftServer.Events;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Status;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Status;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{

    [PacketEvent(typeof(CsStatusRequest), priority: 100)]
    public static void HandleStatusRequest(PacketData data, NetworkConnection connection, Server server)
    {
        ScStatusResponse.Send(new (server.ServerInfo), connection);
    }

    [PacketEvent(typeof(CsStatusPing), priority: 100)]
    public static void HandleStatusPing(CsStatusPingPacketData data, NetworkConnection connection, Server server)
    {
        ScStatusPong.Send(data, connection);
    }

}
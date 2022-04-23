using MinecraftServer.Events;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Play;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{

    [PacketEvent(typeof(CsPlayChatMessage), priority: 100)]
    public static void HandleChatMessage(CsPlayChatMessagePacketData data, NetworkConnection connection, Server server)
    {
        foreach (var player in server.ActiveConnections)
        {
            ScPlayChatMessage.Send(new ScPlayChatMessagePacketData(new ChatComponent($"<{connection.Username}> {data.Message}"), ScPlayChatMessagePacketData.PositionType.Chat, connection.Uuid), player);
        }
    }

    [PacketEvent(typeof(CsPlayKeepAlive), priority: 100)]
    public static void HandleKeepAlive(ScPlayKeepAlivePacketData data, NetworkConnection connection, Server server)
    {
        if (data.KeepAliveId == connection.LastKeepAliveValue)
        {
            connection.LastKeepAlive = DateTime.UtcNow;
        }
    }

}
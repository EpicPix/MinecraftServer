using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{

    public static async Task HandlePacketChatMessage(Server server, NetworkConnection connection, CsPlayChatMessagePacketData data)
    {
        foreach (var player in server.ActiveConnections)
        {
            await ScPlayChatMessage.Send(new ScPlayChatMessagePacketData(new ChatComponent($"<{connection.Username}> {data.Message}"), ScPlayChatMessagePacketData.PositionType.Chat, connection.Uuid), player);
        }
    }

}
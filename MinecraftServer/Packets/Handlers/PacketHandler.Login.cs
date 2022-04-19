using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Login;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{
    public static void HandleLoginStart(Server server, NetworkConnection connection, CsLoginLoginStartPacketData data)
    {
        connection.Username = data.Name;
        if (!server.OnlineMode)
        {
            ScLoginLoginSuccess.Send(new ScLoginLoginSuccessPacketData(Utils.GuidFromString($"OfflinePlayer:{connection.Username}"), connection.Username), connection);
            connection.CurrentState = PacketType.Play;

            ScPlayJoinGame.Send(new ScPlayJoinGamePacketData(), connection);
            // ScLoginDisconnect.Send(new ScLoginDisconnectPacketData(new ChatComponent($"{loginData.Name}")), connection);
            // connection.Connected = false;
            return;
        }
        
    }
}
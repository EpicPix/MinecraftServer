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
            connection.Latency = (DateTime.UtcNow - connection.LastKeepAliveSend).Milliseconds;
        }
    }

    [PacketEvent(typeof(CsPlayPlayerPosition), priority: 100)]
    public static void HandlePosition(CsPlayPlayerPositionPacketData data, NetworkConnection connection, Server server)
    {
        if ((long) connection.PlayerX / 16 != (long) data.X / 16 || (long) connection.PlayerZ / 16 != (long) data.Z / 16)
        {
            ScPlayUpdateViewPosition.Send(new ScPlayUpdateViewPositionPacketData((int) data.X / 16, (int) data.Z / 16), connection);
            
            // very inefficient
            for (var x = -2; x <= 2; x++)
            {
                for (var z = -1; z <= 1; z++)
                {
                    if(!connection.SentChunks.ContainsKey(((long)(data.X / 16) << 32) | (long)(data.Z / 16)))
                    {
                        connection.SentChunks[((long)(data.X / 16) << 32) | (long)(data.Z / 16)] = true;
                        ScPlayChunkDataAndUpdateLight.Send(new ScPlayChunkDataAndUpdateLightPacketData(x + (int)data.X / 16, z + (int)data.Z / 16), connection);
                    }
                }
            }
        } 
        connection.PlayerX = data.X;
        connection.PlayerY = data.Y;
        connection.PlayerZ = data.Z;
    }

    [PacketEvent(typeof(CsPlayPlayerPositionAndRotation), priority: 100)]
    public static void HandlePosition(CsPlayPlayerPositionAndRotationPacketData data, NetworkConnection connection, Server server)
    {
        if ((long) connection.PlayerX / 16 != (long) data.X / 16 || (long) connection.PlayerZ / 16 != (long) data.Z / 16)
        {
            ScPlayUpdateViewPosition.Send(new ScPlayUpdateViewPositionPacketData((int) data.X / 16, (int) data.Z / 16), connection);
            
            // very inefficient
            for (var x = -2; x <= 2; x++)
            {
                for (var z = -1; z <= 1; z++)
                {
                    if (!connection.SentChunks.ContainsKey(((long)(data.X / 16) << 32) | (long)(data.Z / 16)))
                    {
                        connection.SentChunks[((long)(data.X / 16) << 32) | (long)(data.Z / 16)] = true;
                        ScPlayChunkDataAndUpdateLight.Send(new ScPlayChunkDataAndUpdateLightPacketData(x + (int)data.X / 16, z + (int)data.Z / 16), connection);
                    }
                }
            }
        } 
        connection.PlayerX = data.X;
        connection.PlayerY = data.Y;
        connection.PlayerZ = data.Z;
    }
}
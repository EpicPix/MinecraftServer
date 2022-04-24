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
        server.BroadcastMessage(new ChatComponent($"<{connection.Username}> {data.Message}"));
    }

    [PacketEvent(typeof(CsPlayKeepAlive), priority: 100)]
    public static void HandleKeepAlive(ScPlayKeepAlivePacketData data, NetworkConnection connection, Server server)
    {
        if (data.KeepAliveId == connection.LastKeepAliveValue)
        {
            connection.LastKeepAlive = DateTime.UtcNow;
            connection.Player.Ping = (DateTime.UtcNow - connection.LastKeepAliveSend).Milliseconds;
        }
    }

    [PacketEvent(typeof(CsPlayPlayerPosition), priority: 100)]
    public static void HandlePosition(CsPlayPlayerPositionPacketData data, NetworkConnection connection, Server server)
    {
        var player = connection.Player;
        
        if ((long) player.X / 16 != (long) data.X / 16 || (long) player.Z / 16 != (long) data.Z / 16)
        {
            ScPlayUpdateViewPosition.Send(new ScPlayUpdateViewPositionPacketData((int) data.X / 16, (int) data.Z / 16), connection);
            
            // very inefficient
            for (var x = -2; x <= 2; x++)
            {
                for (var z = -1; z <= 1; z++)
                {
                    if(!connection.SentChunks.ContainsKey((x + (int)data.X / 16, x + (int)data.Z / 16)))
                    {
                        connection.SentChunks[(x + (int)data.X / 16, x + (int)data.Z / 16)] = true;
                        ScPlayChunkDataAndUpdateLight.Send(new ScPlayChunkDataAndUpdateLightPacketData(x + (int)data.X / 16, z + (int)data.Z / 16), connection);
                    }
                }
            }
        }

        var deltaX = (data.X * 32 - player.ClientX * 32) * 128;
        var deltaY = (data.Y * 32 - player.ClientY * 32) * 128;
        var deltaZ = (data.Z * 32 - player.ClientZ * 32) * 128;

        player.X = data.X;
        player.Y = data.Y;
        player.Z = data.Z;

        player.ClientX += deltaX / 4096;
        player.ClientY += deltaY / 4096;
        player.ClientZ += deltaZ / 4096;

        foreach (var online in server.Players)
        {
            if (online == connection.Player) continue;
            
            ScPlayPlayerPosition.Send(new ScPlayPlayerPositionPacketData((int) connection.Player.EntityId, (short) deltaX, (short) deltaY, (short) deltaZ, false), online.Connection);
        }
    }

    [PacketEvent(typeof(CsPlayPlayerPositionAndRotation), priority: 100)]
    public static void HandlePosition(CsPlayPlayerPositionAndRotationPacketData data, NetworkConnection connection, Server server)
    {
        var player = connection.Player;
        
        if ((long) player.X / 16 != (long) data.X / 16 || (long) player.Z / 16 != (long) data.Z / 16)
        {
            ScPlayUpdateViewPosition.Send(new ScPlayUpdateViewPositionPacketData((int) data.X / 16, (int) data.Z / 16), connection);
            
            // very inefficient
            for (var x = -2; x <= 2; x++)
            {
                for (var z = -1; z <= 1; z++)
                {
                    if(!connection.SentChunks.ContainsKey((x + (int)data.X / 16, x + (int)data.Z / 16)))
                    {
                        connection.SentChunks[(x + (int)data.X / 16, x + (int)data.Z / 16)] = true;
                        ScPlayChunkDataAndUpdateLight.Send(new ScPlayChunkDataAndUpdateLightPacketData(x + (int)data.X / 16, z + (int)data.Z / 16), connection);
                    }
                }
            }
        } 
        var deltaX = (data.X * 32 - player.ClientX * 32) * 128;
        var deltaY = (data.Y * 32 - player.ClientY * 32) * 128;
        var deltaZ = (data.Z * 32 - player.ClientZ * 32) * 128;

        player.X = data.X;
        player.Y = data.Y;
        player.Z = data.Z;
        player.Yaw = data.Yaw;
        player.Pitch = data.Pitch;

        player.ClientX += deltaX / 4096;
        player.ClientY += deltaY / 4096;
        player.ClientZ += deltaZ / 4096;

        foreach (var online in server.Players)
        {
            if (online == connection.Player) continue;
            
            ScPlayPlayerPositionAndRotation.Send(new ScPlayPlayerPositionAndRotationPacketData((int) connection.Player.EntityId, (short) deltaX, (short) deltaY, (short) deltaZ, (byte) (data.Yaw / 360 * 255), (byte) (data.Pitch / 360 * 255), false), online.Connection);
            ScPlayEntityHeadLook.Send(new ScPlayEntityHeadLookPacketData((int) connection.Player.EntityId, (byte) (data.Yaw / 360 * 255)), online.Connection);
        }
    }

    [PacketEvent(typeof(CsPlayPlayerRotation), priority: 100)]
    public static void HandlePosition(CsPlayPlayerRotationPacketData data, NetworkConnection connection, Server server)
    {
        var player = connection.Player;

        player.Yaw = data.Yaw;
        player.Pitch = data.Pitch;

        foreach (var online in server.Players)
        {
            if (online == connection.Player) continue;
            
            ScPlayPlayerRotation.Send(new ScPlayPlayerRotationPacketData((int) connection.Player.EntityId, (byte) (data.Yaw / 360 * 255), (byte) (data.Pitch / 360 * 255), false), online.Connection);
            ScPlayEntityHeadLook.Send(new ScPlayEntityHeadLookPacketData((int) connection.Player.EntityId, (byte) (data.Yaw / 360 * 255)), online.Connection);
        }
    }

    [PacketEvent(typeof(CsPlayAnimation), priority: 100)]
    public static void HandleAnimation(CsPlayAnimationPacketData data, NetworkConnection connection, Server server)
    {
        var player = connection.Player;

        foreach (var online in server.Players)
        {
            if (online == connection.Player) continue;
            
            ScPlayerEntityAnimation.Send(new ScPlayerEntityAnimationPacketData((int) connection.Player.EntityId, 
                data.Hand == CsPlayAnimationPacketData.UsedHand.MainHand ? 
                ScPlayerEntityAnimationPacketData.AnimationType.SwingMainArm : 
                ScPlayerEntityAnimationPacketData.AnimationType.SwingOffhand), online.Connection);
        }
    }
}
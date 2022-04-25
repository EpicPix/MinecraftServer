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
        if (data.Message.StartsWith("/"))
        {
            var msg = data.Message.Split(" ");
            if (msg[0].Equals("/tp"))
            {
                try
                {
                    var x = int.Parse(msg[1]);
                    var y = int.Parse(msg[2]);
                    var z = int.Parse(msg[3]);
                    connection.Player.Teleport(x, y, z);
                    connection.Player.SendMessage("Done");
                } catch (Exception e)
                {
                    connection.Player.SendMessage($"An exception occurred: {e.Message}");
                }
            } else
            {
                connection.Player.SendMessage("Unknown command");
            }
        } else
        {
            server.BroadcastMessage(new ChatComponent($"<{connection.Username}> {data.Message}"));
        }
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
            
            for (var x = -3; x <= 3; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    if (x * x + z * z < 3 * 3)
                    {
                        if (!connection.SentChunks.ContainsKey((x + (int) data.X / 16, z + (int) data.Z / 16)))
                        {
                            connection.SentChunks[(x + (int) data.X / 16, z + (int) data.Z / 16)] = true;
                            ScPlayChunkDataAndUpdateLight.Send(new ScPlayChunkDataAndUpdateLightPacketData(x + (int) data.X / 16, z + (int) data.Z / 16), connection);
                        }
                    }
                }
            }
        }
        
        player.Move(data.X, data.Y, data.Z);
    }

    [PacketEvent(typeof(CsPlayPlayerPositionAndRotation), priority: 100)]
    public static void HandlePosition(CsPlayPlayerPositionAndRotationPacketData data, NetworkConnection connection, Server server)
    {
        var player = connection.Player;
        
        if ((long) player.X / 16 != (long) data.X / 16 || (long) player.Z / 16 != (long) data.Z / 16)
        {
            ScPlayUpdateViewPosition.Send(new ScPlayUpdateViewPositionPacketData((int) data.X / 16, (int) data.Z / 16), connection);
            
            for (var x = -3; x <= 3; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    if (x * x + z * z < 3 * 3)
                    {
                        if (!connection.SentChunks.ContainsKey((x + (int) data.X / 16, z + (int) data.Z / 16)))
                        {
                            connection.SentChunks[(x + (int) data.X / 16, z + (int) data.Z / 16)] = true;
                            ScPlayChunkDataAndUpdateLight.Send(new ScPlayChunkDataAndUpdateLightPacketData(x + (int) data.X / 16, z + (int) data.Z / 16), connection);
                        }
                    }
                }
            }
        }
        
        player.Move(data.X, data.Y, data.Z);
        player.Rotate(data.Yaw, data.Pitch);
    }

    [PacketEvent(typeof(CsPlayPlayerRotation), priority: 100)]
    public static void HandlePosition(CsPlayPlayerRotationPacketData data, NetworkConnection connection, Server server)
    {
        var player = connection.Player;

        player.Rotate(data.Yaw, data.Pitch);
    }

    [PacketEvent(typeof(CsPlayAnimation), priority: 100)]
    public static void HandleAnimation(CsPlayAnimationPacketData data, NetworkConnection connection, Server server)
    {
        var player = connection.Player;

        foreach (var online in server.Players)
        {
            if (online == connection.Player) continue;
            
            ScPlayEntityAnimation.Send(new ScPlayerEntityAnimationPacketData((int) connection.Player.EntityId, 
                data.Hand == CsPlayAnimationPacketData.UsedHand.MainHand ? 
                    ScPlayerEntityAnimationPacketData.AnimationType.SwingMainArm : 
                    ScPlayerEntityAnimationPacketData.AnimationType.SwingOffhand), online.Connection);
        }
    }

    [PacketEvent(typeof(CsPlayEntityAction), priority: 100)]
    public static void HandleEntityAction(CsPlayEntityActionPacketData data, NetworkConnection connection, Server server, ref PacketEventHandlerStatus status)
    {
        var player = connection.Player;
        if (data.EntityId != player.EntityId)
        {
            status = PacketEventHandlerStatus.Stop;
            return;
        }

        if (data.Action == CsPlayEntityActionPacketData.ActionType.StartSneaking)
        {
            player.Sneaking = true;
            player.SetPose(Player.EntityPose.Sneaking);
        }
        else if (data.Action == CsPlayEntityActionPacketData.ActionType.StopSneaking)
        {
            player.Sneaking = false;
            player.SetPose(Player.EntityPose.Standing);
        }
    }
}
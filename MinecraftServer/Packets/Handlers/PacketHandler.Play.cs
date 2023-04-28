using MinecraftServer.Data;
using MinecraftServer.Events;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.SourceGenerators.Events;
using MinecraftServer.Types;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{

    [EventHandler(EventBuses.Packet, typeof(CsPlayChatMessagePacketData), priority: 100)]
    public static void HandleChatMessage(CsPlayChatMessagePacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
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

    [EventHandler(EventBuses.Packet, typeof(CsPlayKeepAlivePacketData), priority: 100)]
    public static void HandleKeepAlive(CsPlayKeepAlivePacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        if (data.KeepAliveId == connection.LastKeepAliveValue)
        {
            connection.LastKeepAlive = DateTime.UtcNow;
            connection.Player.Ping = (DateTime.UtcNow - connection.LastKeepAliveSend).Milliseconds;
        }
    }

    [EventHandler(EventBuses.Packet, typeof(CsPlayPlayerPositionPacketData), priority: 100)]
    public static void HandlePosition(CsPlayPlayerPositionPacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        var player = connection.Player;
        
        if ((long) player.X / 16 != (long) data.X / 16 || (long) player.Z / 16 != (long) data.Z / 16)
        {
            for (var x = -3; x <= 3; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    if (x * x + z * z < 3 * 3)
                    {
                        if (!connection.SentChunks.ContainsKey((x + (int) data.X / 16, z + (int) data.Z / 16)))
                        {
//                            connection.SentChunks[(x + (int) data.X / 16, z + (int) data.Z / 16)] = true;
//                            ScPlayChunkDataAndUpdateLight.Send(new ScPlayChunkDataAndUpdateLightPacketData(x + (int) data.X / 16, z + (int) data.Z / 16, server.GetChunk(x + (int) data.X / 16, z + (int) data.Z / 16)), connection);
                        }
                    }
                }
            }
        }
        
        player.Move(data.X, data.Y, data.Z);
    }

    [EventHandler(EventBuses.Packet, typeof(CsPlayPlayerPositionAndRotationPacketData), priority: 100)]
    public static void HandlePosition(CsPlayPlayerPositionAndRotationPacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        var player = connection.Player;
        
        if ((long) player.X / 16 != (long) data.X / 16 || (long) player.Z / 16 != (long) data.Z / 16)
        {
            for (var x = -3; x <= 3; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    if (x * x + z * z < 3 * 3)
                    {
                        if (!connection.SentChunks.ContainsKey((x + (int) data.X / 16, z + (int) data.Z / 16)))
                        {
//                            connection.SentChunks[(x + (int) data.X / 16, z + (int) data.Z / 16)] = true;
//                            ScPlayChunkDataAndUpdateLight.Send(new ScPlayChunkDataAndUpdateLightPacketData(x + (int) data.X / 16, z + (int) data.Z / 16, server.GetChunk(x + (int) data.X / 16, z + (int) data.Z / 16)), connection);
                        }
                    }
                }
            }
        }
        
        player.Move(data.X, data.Y, data.Z);
        player.Rotate(data.Yaw, data.Pitch);
    }

    [EventHandler(EventBuses.Packet, typeof(CsPlayPlayerRotationPacketData), priority: 100)]
    public static void HandlePosition(CsPlayPlayerRotationPacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        var player = connection.Player;

        player.Rotate(data.Yaw, data.Pitch);
    }

    [EventHandler(EventBuses.Packet, typeof(CsPlayAnimationPacketData), priority: 100)]
    public static void HandleAnimation(CsPlayAnimationPacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
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

    [EventHandler(EventBuses.Packet, typeof(CsPlayEntityActionPacketData), priority: 100)]
    public static void HandleEntityAction(CsPlayEntityActionPacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        var player = connection.Player;
        if (data.EntityId != player.EntityId)
        {
            bus.Cancel();
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

    [EventHandler(EventBuses.Packet, typeof(CsPlayPlayerDiggingPacketData), priority: 100)]
    public static void HandleDigging(CsPlayPlayerDiggingPacketData data, PacketEventBus bus)
    {
        if (data.Status == CsPlayPlayerDiggingPacketData.DiggingStatus.StartedDigging)
        {
            // Means broke block in creative
            var pos = data.Position;
            var chunk = bus.Server.GetChunk(pos.X / 16, pos.Z / 16);
            chunk[(BlockSignedToUnsigned(pos.X), (byte) pos.Y, BlockSignedToUnsigned(pos.Z))] = BlockState.Air;
            foreach (var player in bus.Server.Players)
            {
                ScPlayBlockChange.Send(new ScPlayBlockChangePacketData(
                    pos,
                    BlockState.Air
                ), player.Connection);
            }
        }
    }
    
    private static byte BlockSignedToUnsigned(int loc)
    {
        var x = (uint) loc;
        // if (loc < 0)
        // {
        //     // return (byte) (loc % 16 + 16);
        //     return (byte) (-loc % 16);
        // }
        if (loc < 0)
        {
            x -= 16;
        }
        return (byte) (x % 16);
    }
}
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer;

public class Player
{

    public Server Server { get; }
    
    public uint EntityId { get; }
    public Guid Uuid { get; }
    
    public NetworkConnection Connection { get; }

    private int _ping;
    public int Ping {
        get => _ping;
        set {
            Console.WriteLine($"Ping for {Username} set to {value}ms");
            _ping = value;
        }
    }
    
    public string Username { get; }
    
    public double X;
    public double Y;
    public double Z;

    public float Yaw = 0;
    public float Pitch = 0;
    

    public double ClientX;
    public double ClientY;
    public double ClientZ;

    public Player(Server server, NetworkConnection connection, uint entityId)
    {
        Server = server;
        Connection = connection;
        Username = connection.Username;
        Uuid = connection.Uuid;
        EntityId = entityId;
    }

    public void SendMessage(ChatComponent component, Guid sender, ScPlayChatMessagePacketData.PositionType position = ScPlayChatMessagePacketData.PositionType.Chat)
    {
        ScPlayChatMessage.Send(new ScPlayChatMessagePacketData(component, position, sender), Connection);
    }

    public void SendMessage(ChatComponent component, ScPlayChatMessagePacketData.PositionType position = ScPlayChatMessagePacketData.PositionType.Chat) => SendMessage(component, Guid.Empty, position);
    public void SendMessage(string text, ScPlayChatMessagePacketData.PositionType position = ScPlayChatMessagePacketData.PositionType.Chat) => SendMessage(new ChatComponent(text), Guid.Empty, position);
    public void SendMessage(string text, Guid sender) => SendMessage(new ChatComponent(text), sender);

    public void Rotate(float yaw, float pitch)
    {
        Yaw = yaw;
        Pitch = pitch;

        foreach (var online in Server.Players)
        {
            if (online == this) continue;
            
            ScPlayPlayerRotation.Send(new ScPlayPlayerRotationPacketData((int) EntityId, (byte) (Yaw / 360 * 255), (byte) (Pitch / 360 * 255), false), online.Connection);
            ScPlayEntityHeadLook.Send(new ScPlayEntityHeadLookPacketData((int) EntityId, (byte) (Yaw / 360 * 255)), online.Connection);
        }
    }
    
    public void Move(double x, double y, double z, float? yaw = null, float? pitch = null)
    {
        var deltaX = (x * 32 - ClientX * 32) * 128;
        var deltaY = (y * 32 - ClientY * 32) * 128;
        var deltaZ = (z * 32 - ClientZ * 32) * 128;

        X = x;
        Y = y;
        Z = z;

        if (yaw != null) Yaw = yaw.Value;
        if (pitch != null) Pitch = pitch.Value;

        var highestDelta = Math.Max(Math.Max(Math.Abs(deltaX), Math.Abs(deltaY)), Math.Abs(deltaZ));

        if (highestDelta > 4096 * 8) // cannot travel more than 8 blocks in any direction using delta, so we teleport instead
        {
            foreach (var online in Server.Players)
            {
                if (online == this) continue;

                ScPlayEntityTeleport.Send(new ScPlayEntityTeleportPacketData((int) EntityId, X, Y, Z, (byte) (Yaw / 360 * 256), (byte) (Pitch / 360 * 256), false), online.Connection);
            }
        }
        else
        {

            ClientX += deltaX / 4096;
            ClientY += deltaY / 4096;
            ClientZ += deltaZ / 4096;

            var sendRotationPacket = yaw != null || pitch != null;

            foreach (var online in Server.Players)
            {
                if (online == this) continue;

                if (sendRotationPacket)
                {
                    ScPlayPlayerPositionAndRotation.Send(new ScPlayPlayerPositionAndRotationPacketData((int) EntityId, (short) deltaX, (short) deltaY, (short) deltaZ, (byte) (Yaw / 360 * 255), (byte) (Pitch / 360 * 255), false), online.Connection);
                    ScPlayEntityHeadLook.Send(new ScPlayEntityHeadLookPacketData((int) EntityId, (byte) (Yaw / 360 * 255)), online.Connection);
                } else
                {
                    ScPlayPlayerPosition.Send(new ScPlayPlayerPositionPacketData((int) EntityId, (short) deltaX, (short) deltaY, (short) deltaZ, false), online.Connection);
                }
            }
        }
    }

}
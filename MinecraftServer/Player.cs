using MinecraftServer.EntityMetadata;
using MinecraftServer.Networking;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer;

public class Player : ITickable
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
    
    public double X { get; internal set; }
    public double Y { get; internal set; }
    public double Z { get; internal set; }
    public bool ChangedPosition { get; internal set; }
    public bool HasTeleported { get; internal set; }

    public float Yaw { get; internal set; }
    public float Pitch { get; internal set; }
    public bool ChangedRotation { get; internal set; }

    public double ClientX { get; internal set; }
    public double ClientY { get; internal set; }
    public double ClientZ { get; internal set; }

    public byte EntityFlags { get; internal set; }
    public bool ChangedFlags { get; internal set; }

    public int Pose { get; internal set; }
    public bool ChangedPose { get; internal set; }

    public bool Sneaking {
        get => (EntityFlags & 0x02) == 0x02;
        set {
            if (value)
            {
                SetEntityFlags((byte) (EntityFlags | 0x02));
            } else
            {
                SetEntityFlags((byte) (EntityFlags & ~0x02));
            }
        }
    }

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

    public void Tick()
    {

        bool teleport = HasTeleported;
        double deltaX = 0, deltaY = 0, deltaZ = 0;

        if (ChangedPosition && !HasTeleported)
        {
            deltaX = (X * 32 - ClientX * 32) * 128;
            deltaY = (Y * 32 - ClientY * 32) * 128;
            deltaZ = (Z * 32 - ClientZ * 32) * 128;
        
            var highestDelta = Math.Max(Math.Max(Math.Abs(deltaX), Math.Abs(deltaY)), Math.Abs(deltaZ));
        
            if (highestDelta > 4096 * 8) // cannot travel more than 8 blocks in any direction using delta, so we teleport instead
            {
                teleport = true;
            }
            else
            {
        
                ClientX += deltaX / 4096;
                ClientY += deltaY / 4096;
                ClientZ += deltaZ / 4096;
            }
        }

        if (HasTeleported)
        {
            ScPlayPlayerPositionAndLook.Send(new ScPlayPlayerPositionAndLookPacketData(X, Y, Z, Yaw, Pitch, 0x0, 0, false), Connection);
            HasTeleported = false;
        }
        
        foreach (var online in Server.Players)
        {
            if (online == this) continue;
            
            if(teleport) {
                ScPlayEntityTeleport.Send(new ScPlayEntityTeleportPacketData((int) EntityId, X, Y, Z, (byte) (Yaw / 360 * 256), (byte) (Pitch / 360 * 256), false), online.Connection);
            }else if (ChangedPosition && ChangedRotation)
            {
                ScPlayPlayerPositionAndRotation.Send(new ScPlayPlayerPositionAndRotationPacketData((int) EntityId, (short) deltaX, (short) deltaY, (short) deltaZ, (byte) (Yaw / 360 * 255), (byte) (Pitch / 360 * 255), false), online.Connection);
            }else if (ChangedPosition && !ChangedRotation)
            {
                ScPlayPlayerPosition.Send(new ScPlayPlayerPositionPacketData((int) EntityId, (short) deltaX, (short) deltaY, (short) deltaZ, false), online.Connection);
            } else
            {
                ScPlayPlayerRotation.Send(new ScPlayPlayerRotationPacketData((int) EntityId, (byte) (Yaw / 360 * 255), (byte) (Pitch / 360 * 255), false), online.Connection);
            }

            if (ChangedRotation)
            {
                ScPlayEntityHeadLook.Send(new ScPlayEntityHeadLookPacketData((int) EntityId, (byte) (Yaw / 360 * 255)), online.Connection);
            }
        }


        var changedMetadataValues = 0;
        if (ChangedFlags) changedMetadataValues++;
        if (ChangedPose) changedMetadataValues++;
        
        var metadata = new Tuple<byte, IMetadataValue>[changedMetadataValues];
        var index = 0;
        if (ChangedFlags)
        {
            metadata[index++] = new Tuple<byte, IMetadataValue>(0, new MetadataByte(EntityFlags));
            ChangedFlags = false;
        }
        if (ChangedPose)
        {
            metadata[index++] = new Tuple<byte, IMetadataValue>(6, new MetadataPose(Pose));
            ChangedPose = false;
        }

        if (changedMetadataValues != 0)
        {
            foreach (var online in Server.Players)
            {
                ScPlayEntityMetadata.Send(new ScPlayEntityMetadataPacketData((int) EntityId, metadata), online.Connection);
            }
        }

        ChangedRotation = false;
        ChangedPosition = false;
    }

    public void Rotate(float yaw, float pitch)
    {
        Yaw = yaw;
        Pitch = pitch;
        ChangedRotation = true;
    }
    
    public void Move(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
        ChangedPosition = true;
    }
    
    public void Teleport(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
        ChangedPosition = true;
        HasTeleported = true;
    }
    
    public void Teleport(double x, double y, double z, float yaw, float pitch)
    {
        X = x;
        Y = y;
        Z = z;
        Yaw = yaw;
        Pitch = pitch;
        ChangedPosition = true;
        ChangedRotation = true;
        HasTeleported = true;
    }

    public void SetEntityFlags(byte flags)
    {
        EntityFlags = flags;
        ChangedFlags = true;
    }
    
    public void SetPose(int pose) // add enums later
    {
        Pose = pose;
        ChangedPose = true;
    }
}
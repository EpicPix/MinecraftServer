namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayEntityTeleportPacketData : PacketData
{

    public int EntityId { get; }
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public byte Yaw { get; }
    public byte Pitch { get; }
    public bool OnGround { get; }

    public ScPlayEntityTeleportPacketData(int entityId, double x, double y, double z, byte yaw, byte pitch, bool onGround)
    {
        EntityId = entityId;
        X = x;
        Y = y;
        Z = z;
        Yaw = yaw;
        Pitch = pitch;
        OnGround = onGround;
    }

    public override string ToString()
    {
        return $"ScPlayEntityTeleportPacketData[EntityId={EntityId},X={X},Y={Y},Z={Z},Yaw={Yaw},Pitch={Pitch},OnGround={OnGround}]";
    }

}
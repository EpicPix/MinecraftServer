namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlaySpawnPlayerPacketData : PacketData
{

    public int EntityId { get; }
    public Guid Uuid { get; }
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public byte Yaw { get; }
    public byte Pitch { get; }

    public ScPlaySpawnPlayerPacketData(int entityId, Guid uuid, double x, double y, double z, byte yaw, byte pitch)
    {
        EntityId = entityId;
        Uuid = uuid;
        X = x;
        Y = y;
        Z = z;
        Yaw = yaw;
        Pitch = pitch;
    }

}
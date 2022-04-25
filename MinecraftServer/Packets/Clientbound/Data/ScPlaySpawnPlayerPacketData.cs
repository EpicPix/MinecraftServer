using MinecraftServer.Data;
using MinecraftServer.Types;

namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlaySpawnPlayerPacketData : PacketData
{

    public int EntityId { get; }
    public Guid Uuid { get; }
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public Angle Yaw { get; }
    public Angle Pitch { get; }

    public ScPlaySpawnPlayerPacketData(int entityId, Guid uuid, double x, double y, double z, Angle yaw, Angle pitch)
    {
        EntityId = entityId;
        Uuid = uuid;
        X = x;
        Y = y;
        Z = z;
        Yaw = yaw;
        Pitch = pitch;
    }

    public override string ToString()
    {
        return $"ScPlaySpawnPlayerPacketData[EntityId={EntityId},Uuid={Uuid},X={X},Y={Y},Z={Z},Yaw={Yaw},Pitch={Pitch}]";
    }

}
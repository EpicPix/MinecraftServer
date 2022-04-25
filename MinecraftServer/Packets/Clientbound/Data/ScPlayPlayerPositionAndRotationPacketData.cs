using System.Text;
using MinecraftServer.Data;
using MinecraftServer.Types;

namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayPlayerPositionAndRotationPacketData : PacketData
{

    public int EntityId { get; }
    public short DeltaX { get; }
    public short DeltaY { get; }
    public short DeltaZ { get; }
    public Angle Yaw { get; }
    public Angle Pitch { get; }
    public bool OnGround { get; }

    public ScPlayPlayerPositionAndRotationPacketData(int entityId, short deltaX, short deltaY, short deltaZ, Angle yaw, Angle pitch, bool onGround)
    {
        EntityId = entityId;
        DeltaX = deltaX;
        DeltaY = deltaY;
        DeltaZ = deltaZ;
        Yaw = yaw;
        Pitch = pitch;
        OnGround = onGround;
    }

    public override string ToString()
    {
        return $"ScPlayPlayerPositionAndRotationPacketData[EntityId={EntityId},DeltaX={DeltaX},DeltaY={DeltaY},DeltaZ={DeltaZ},Yaw={Yaw},Pitch={Pitch},OnGround={OnGround}]";
    }

}
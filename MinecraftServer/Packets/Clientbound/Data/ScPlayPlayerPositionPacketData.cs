using System.Text;

namespace MinecraftServer.Packets.Serverbound.Data;

public class ScPlayPlayerPositionPacketData : PacketData
{

    public int EntityId { get; }
    public short DeltaX { get; }
    public short DeltaY { get; }
    public short DeltaZ { get; }
    public bool OnGround { get; }

    public ScPlayPlayerPositionPacketData(int entityId, short deltaX, short deltaY, short deltaZ, bool onGround)
    {
        EntityId = entityId;
        DeltaX = deltaX;
        DeltaY = deltaY;
        DeltaZ = deltaZ;
        OnGround = onGround;
    }

    public override string ToString()
    {
        return $"ScPlayPlayerPositionPacketData[EntityId={EntityId},DeltaX={DeltaX},DeltaY={DeltaY},DeltaZ={DeltaZ},OnGround={OnGround}]";
    }

}
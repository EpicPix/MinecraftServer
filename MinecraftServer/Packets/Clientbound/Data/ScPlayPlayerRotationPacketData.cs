using System.Text;
using MinecraftServer.Data;
using MinecraftServer.Types;

namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayPlayerRotationPacketData : PacketData
{

    public int EntityId { get; }
    public Angle Yaw { get; }
    public Angle Pitch { get; }
    public bool OnGround { get; }

    public ScPlayPlayerRotationPacketData(int entityId, Angle yaw, Angle pitch, bool onGround)
    {
        EntityId = entityId;
        Yaw = yaw;
        Pitch = pitch;
        OnGround = onGround;
    }

    public override string ToString()
    {
        return $"ScPlayPlayerRotationPacketData[EntityId={EntityId},Yaw={Yaw},Pitch={Pitch},OnGround={OnGround}]";
    }

}
using System.Text;
using MinecraftServer.Data;

namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayEntityHeadLookPacketData : PacketData
{

    public int EntityId { get; }
    public Angle HeadYaw { get; }

    public ScPlayEntityHeadLookPacketData(int entityId, Angle headYaw)
    {
        EntityId = entityId;
        HeadYaw = headYaw;
    }

    public override string ToString()
    {
        return $"ScPlayEntityHeadLookPacketData[EntityId={EntityId},HeadYaw={HeadYaw}]";
    }

}
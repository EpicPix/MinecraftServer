using System.Text;

namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayEntityHeadLookPacketData : PacketData
{

    public int EntityId { get; }
    public byte HeadYaw { get; }

    public ScPlayEntityHeadLookPacketData(int entityId, byte headYaw)
    {
        EntityId = entityId;
        HeadYaw = headYaw;
    }

    public override string ToString()
    {
        return $"ScPlayEntityHeadLookPacketData[EntityId={EntityId},HeadYaw={HeadYaw}]";
    }

}
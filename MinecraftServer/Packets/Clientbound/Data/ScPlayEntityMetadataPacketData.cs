using MinecraftServer.EntityMetadata;

namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayEntityMetadataPacketData : PacketData
{

    public int EntityId { get; }
    public Tuple<byte, IMetadataValue>[] Metadata { get; }

    public ScPlayEntityMetadataPacketData(int entityId, params Tuple<byte, IMetadataValue>[] metadata)
    {
        EntityId = entityId;
        Metadata = metadata;
    }

    public override string ToString()
    {
        return $"ScPlayEntityMetadataPacketData[EntityId={EntityId},Metadata={Metadata}]";
    }

}
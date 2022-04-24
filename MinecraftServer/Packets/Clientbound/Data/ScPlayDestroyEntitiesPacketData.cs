namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayDestroyEntitiesPacketData : PacketData
{
    public IReadOnlyList<int> Entities { get; }

    public ScPlayDestroyEntitiesPacketData(List<int> entities)
    {
        Entities = entities.AsReadOnly();
    }
}
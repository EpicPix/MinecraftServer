namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayPlayerInfoRemovePacketData : PacketData
{
    public IReadOnlyList<Guid> PlayerIds;

    public ScPlayPlayerInfoRemovePacketData(List<Guid> playerIds)
    {
        PlayerIds = playerIds.AsReadOnly();
    }

}
namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayJoinGamePacketData : PacketData
{

    public int EntityId { get; }

    public ScPlayJoinGamePacketData(int entityId)
    {
        EntityId = entityId;
    }

}
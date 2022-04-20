namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayKeepAlivePacketData : PacketData
{
    public long KeepAliveId { get; set; }
}
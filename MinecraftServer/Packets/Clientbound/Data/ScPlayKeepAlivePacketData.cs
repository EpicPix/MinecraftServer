namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayKeepAlivePacketData : PacketData
{
    public long KeepAliveId { get; set; }

    public ScPlayKeepAlivePacketData(long keepAliveId)
    {
        KeepAliveId = keepAliveId;
    }

    public override string ToString()
    {
        return $"ScPlayKeepAlivePacketData[KeepAliveId=0x{KeepAliveId:x}]";
    }
}
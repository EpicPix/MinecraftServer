namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayKeepAlivePacketData : PacketData
{
    public long KeepAliveId { get; set; }

    public CsPlayKeepAlivePacketData(long keepAliveId)
    {
        KeepAliveId = keepAliveId;
    }

    public override string ToString()
    {
        return $"CsPlayKeepAlivePacketData[KeepAliveId=0x{KeepAliveId:x}]";
    }
}
namespace MinecraftServer.Packets.Serverbound.Data;

public class CsStatusPingPacketData : PacketData
{
    public ulong Payload { get; }

    public CsStatusPingPacketData(ulong payload)
    {
        Payload = payload;
    }
}
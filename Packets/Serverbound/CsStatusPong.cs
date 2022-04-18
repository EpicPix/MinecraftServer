using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound;

public class CsStatusPong : Packet<CsStatusPingPacketData, CsStatusPong>
{
    public override PacketType Type => PacketType.Status;
    public override PacketSide Side => PacketSide.Server;
    public override uint Id => 1;

    public override async Task<CsStatusPingPacketData> ReadPacket(NetworkConnection stream)
    {
        return new CsStatusPingPacketData(await stream.ReadULong());
    }

    public override async Task WritePacket(NetworkConnection stream, CsStatusPingPacketData data)
    {
        await stream.WriteULong(data.Payload);
    }
}
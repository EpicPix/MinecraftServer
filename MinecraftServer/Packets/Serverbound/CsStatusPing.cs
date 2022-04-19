using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound;

public class CsStatusPing : Packet<CsStatusPingPacketData, CsStatusPing>
{
    public override PacketType Type => PacketType.Status;
    public override PacketSide Side => PacketSide.Client;
    public override uint Id => 1;

    public override async Task<PacketData> ReadPacket(NetworkConnection stream)
    {
        return new CsStatusPingPacketData(await stream.ReadULong());
    }

    public override async Task WritePacket(NetworkConnection stream, PacketData data)
    {
        await stream.WriteULong(Of(data).Payload);
    }
}
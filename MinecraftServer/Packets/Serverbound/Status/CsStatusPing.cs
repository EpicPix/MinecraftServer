using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Status;

public class CsStatusPing : Packet<CsStatusPing, CsStatusPingPacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 1;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        return new CsStatusPingPacketData(await stream.ReadULong());
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        await stream.WriteULong(Of(data).Payload);
    }
}
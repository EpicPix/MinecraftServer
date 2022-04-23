using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Status;

public class CsStatusPing : Packet<CsStatusPing, CsStatusPingPacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 1;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        // this does not follow the described packet on wiki.vg
        return ValueTask.FromResult<PacketData>(new CsStatusPingPacketData(0ul));
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        await stream.WriteULong(Of(data).Payload);
    }
}
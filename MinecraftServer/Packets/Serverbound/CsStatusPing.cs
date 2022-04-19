using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound;

public class CsStatusPing : Packet<CsStatusPing, CsStatusPingPacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketSide Side => PacketSide.Client;
    public override uint Id => 1;

    public override PacketData ReadPacket(NetworkConnection stream)
    {
        return new CsStatusPingPacketData(stream.ReadULong());
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        stream.WriteULong(Of(data).Payload);
    }
}
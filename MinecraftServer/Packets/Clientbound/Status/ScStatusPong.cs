using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Clientbound.Status;

public class ScStatusPong : Packet<ScStatusPong, CsStatusPingPacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketSide Side => PacketSide.Server;
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
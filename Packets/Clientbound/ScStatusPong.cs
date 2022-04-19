using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Clientbound;

public class ScStatusPong : Packet<CsStatusPingPacketData, ScStatusPong>
{
    public override PacketType Type => PacketType.Status;
    public override PacketSide Side => PacketSide.Server;
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
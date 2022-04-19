using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Clientbound.Status;

public class ScStatusPong : Packet<ScStatusPong, CsStatusPingPacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 1;

    public override async ValueTask<PacketData> ReadPacket(NetworkConnection stream)
    {
        return new CsStatusPingPacketData(await stream.ReadULong());
    }

    public override async ValueTask WritePacket(NetworkConnection stream, PacketData data)
    {
        await stream.WriteULong(Of(data).Payload);
    }
}
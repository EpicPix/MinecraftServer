namespace MinecraftServer.Packets.Serverbound.Status;

public class CsStatusRequest : Packet<CsStatusRequest, PacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0;

    public override ValueTask<PacketData> ReadPacket(NetworkConnection stream)
    {
        return new ValueTask<PacketData>(new PacketData());
    }

    public override ValueTask WritePacket(NetworkConnection stream, PacketData data)
    {
        return ValueTask.CompletedTask;
    }
}
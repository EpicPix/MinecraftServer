namespace MinecraftServer.Packets.Serverbound.Status;

public class CsStatusRequest : Packet<CsStatusRequest, PacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        return new ValueTask<PacketData>(new PacketData());
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        return ValueTask.CompletedTask;
    }
}
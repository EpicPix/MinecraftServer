namespace MinecraftServer.Packets.Serverbound;

public class CsStatusRequest : Packet<PacketData, CsStatusRequest>
{
    public override PacketType Type => PacketType.Status;
    public override PacketSide Side => PacketSide.Client;
    public override uint Id => 0;

    public override Task<PacketData> ReadPacket(NetworkConnection stream)
    {
        return Task.FromResult(new PacketData());
    }

    public override Task WritePacket(NetworkConnection stream, PacketData data)
    {
        return Task.CompletedTask;
    }
}
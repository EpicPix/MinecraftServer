namespace MinecraftServer.Packets.Serverbound.Status;

public class CsStatusRequest : Packet<CsStatusRequest, PacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0;

    public override PacketData ReadPacket(NetworkConnection stream)
    {
        return new PacketData();
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        
    }
}
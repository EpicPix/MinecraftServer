namespace MinecraftServer.Packets.Serverbound;

public class CsStatusRequest : Packet<CsStatusRequest, PacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketSide Side => PacketSide.Client;
    public override uint Id => 0;

    public override PacketData ReadPacket(NetworkConnection stream)
    {
        return new PacketData();
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        
    }
}
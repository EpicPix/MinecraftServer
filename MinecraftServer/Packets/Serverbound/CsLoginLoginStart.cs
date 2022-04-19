using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound;

public class CsLoginLoginStart : Packet<CsLoginLoginStart, CsLoginLoginStartPacketData>
{

    public override PacketType Type => PacketType.Login;
    public override PacketSide Side => PacketSide.Client;
    public override uint Id => 0;

    public override PacketData ReadPacket(NetworkConnection stream)
    {
        return new CsLoginLoginStartPacketData(stream.ReadString(16));
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        stream.WriteString(Of(data).Name, 16);
    }
}
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Login;

public class CsLoginLoginStart : Packet<CsLoginLoginStart, CsLoginLoginStartPacketData>
{

    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Server;
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
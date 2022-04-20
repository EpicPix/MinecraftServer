using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Login;

public class CsLoginLoginStart : Packet<CsLoginLoginStart, CsLoginLoginStartPacketData>
{

    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        return new CsLoginLoginStartPacketData(await stream.ReadString(16));
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        await stream.WriteString(Of(data).Name, 16);
    }
}
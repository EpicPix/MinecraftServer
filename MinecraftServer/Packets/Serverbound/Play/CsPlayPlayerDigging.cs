using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayPlayerDigging : Packet<CsPlayPlayerDigging, CsPlayPlayerDiggingPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x1D;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var status = (CsPlayPlayerDiggingPacketData.DiggingStatus) await stream.ReadVarIntAsync();
        var position = await stream.ReadPositionAsync();
        var face = await stream.ReadByteAsync();
        var sequence = await stream.ReadVarIntAsync();
        return new CsPlayPlayerDiggingPacketData(status, position, face);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }
}
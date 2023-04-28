using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayPlayerPosition : Packet<CsPlayPlayerPosition, CsPlayPlayerPositionPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x14;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var x = await stream.ReadDoubleAsync();
        var y = await stream.ReadDoubleAsync();
        var z = await stream.ReadDoubleAsync();
        var onGround = await stream.ReadBoolAsync();
        
        return new CsPlayPlayerPositionPacketData(x, y, z, onGround);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }
}
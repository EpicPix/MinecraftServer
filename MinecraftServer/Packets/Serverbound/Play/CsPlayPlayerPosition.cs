using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayPlayerPosition : Packet<CsPlayPlayerPosition, CsPlayPlayerPositionPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x11;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var x = await stream.ReadDouble();
        var y = await stream.ReadDouble();
        var z = await stream.ReadDouble();
        var onGround = await stream.ReadBool();
        
        return new CsPlayPlayerPositionPacketData(x, y, z, onGround);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }
}
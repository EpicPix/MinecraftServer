using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayPlayerPositionAndRotation : Packet<CsPlayPlayerPositionAndRotation, CsPlayPlayerPositionAndRotationPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x12;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var x = await stream.ReadDouble();
        var y = await stream.ReadDouble();
        var z = await stream.ReadDouble();
        var yaw = await stream.ReadFloat();
        var pitch = await stream.ReadFloat();
        var onGround = await stream.ReadBool();
        
        return new CsPlayPlayerPositionAndRotationPacketData(x, y, z, yaw, pitch, onGround);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }
}
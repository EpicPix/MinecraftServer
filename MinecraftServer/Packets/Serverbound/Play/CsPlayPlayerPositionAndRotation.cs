using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayPlayerPositionAndRotation : Packet<CsPlayPlayerPositionAndRotation, CsPlayPlayerPositionAndRotationPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x15;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var x = await stream.ReadDoubleAsync();
        var y = await stream.ReadDoubleAsync();
        var z = await stream.ReadDoubleAsync();
        var yaw = await stream.ReadFloatAsync();
        var pitch = await stream.ReadFloatAsync();
        var onGround = await stream.ReadBoolAsync();
        
        return new CsPlayPlayerPositionAndRotationPacketData(x, y, z, yaw, pitch, onGround);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }
}
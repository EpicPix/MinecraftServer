using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayPlayerRotation : Packet<CsPlayPlayerRotation, CsPlayPlayerRotationPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x16;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var yaw = await stream.ReadFloatAsync();
        var pitch = await stream.ReadFloatAsync();
        var onGround = await stream.ReadBoolAsync();
        
        return new CsPlayPlayerRotationPacketData(yaw, pitch, onGround);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }
}
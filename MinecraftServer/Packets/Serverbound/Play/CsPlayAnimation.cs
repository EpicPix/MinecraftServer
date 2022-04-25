using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayAnimation : Packet<CsPlayAnimation, CsPlayAnimationPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x2C;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        return new CsPlayAnimationPacketData((CsPlayAnimationPacketData.UsedHand) await stream.ReadVarIntAsync());
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }

}
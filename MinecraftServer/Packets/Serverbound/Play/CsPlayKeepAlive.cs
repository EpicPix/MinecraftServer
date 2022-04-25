using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayKeepAlive : Packet<CsPlayKeepAlive, ScPlayKeepAlivePacketData>
{
    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x0F;
    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var id = (long)await stream.ReadUnsignedLongAsync();
        return new ScPlayKeepAlivePacketData(id);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        return stream.WriteUnsignedLongAsync((ulong)Of(data).KeepAliveId);
    }
}
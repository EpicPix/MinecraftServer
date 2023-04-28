using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayKeepAlive : Packet<CsPlayKeepAlive, CsPlayKeepAlivePacketData>
{
    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x12;
    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var id = (long)await stream.ReadUnsignedLongAsync();
        return new CsPlayKeepAlivePacketData(id);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        return stream.WriteUnsignedLongAsync((ulong)Of(data).KeepAliveId);
    }
}
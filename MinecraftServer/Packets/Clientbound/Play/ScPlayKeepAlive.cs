using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayKeepAlive : Packet<ScPlayKeepAlive, ScPlayKeepAlivePacketData>
{
    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x23;
    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var keepAliveId = (long)await stream.ReadUnsignedLongAsync();
        return new ScPlayKeepAlivePacketData(keepAliveId);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        return stream.WriteUnsignedLongAsync((ulong)Of(data).KeepAliveId);
    }
}
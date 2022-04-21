using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayKeepAlive : Packet<ScPlayKeepAlive, ScPlayKeepAlivePacketData>
{
    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x21;
    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var keepAliveId = (long)await stream.ReadULong();
        return new ScPlayKeepAlivePacketData(keepAliveId);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        return stream.WriteULong((ulong)Of(data).KeepAliveId);
    }
}
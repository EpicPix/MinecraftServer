using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Login;

public class ScLoginSetCompression : Packet<ScLoginSetCompression, ScLoginSetCompressionPacketData>
{

    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x04;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        return new ScLoginSetCompressionPacketData(await stream.ReadVarInt());
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        await stream.WriteVarInt(Of(data).CompressionThreshold);
    }
}
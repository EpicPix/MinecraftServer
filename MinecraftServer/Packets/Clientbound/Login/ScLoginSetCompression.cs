using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Login;

public class ScLoginSetCompression : Packet<ScLoginSetCompression, ScLoginSetCompressionPacketData>
{

    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x03;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        return new ScLoginSetCompressionPacketData(await stream.ReadVarIntAsync());
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        await stream.WriteVarIntAsync(Of(data).CompressionThreshold);
    }
}
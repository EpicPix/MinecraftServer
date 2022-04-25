using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayChatMessage : Packet<CsPlayChatMessage, CsPlayChatMessagePacketData>
{
    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x03;
    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var message = await stream.ReadStringAsync(32768);
        return new CsPlayChatMessagePacketData(message);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        return stream.WriteStringAsync(Of(data).Message, 32768);
    }
}
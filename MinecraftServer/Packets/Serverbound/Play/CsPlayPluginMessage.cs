using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayPluginMessage : Packet<CsPlayPluginMessage, CsPlayPluginMessagePacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x0A;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var identifier = await stream.ReadStringAsync(ushort.MaxValue);
        var bytes = IoBuffer.Allocate((int)stream.Length);
        await stream.ReadBytesAsync(bytes);
        return new CsPlayPluginMessagePacketData(identifier, bytes);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }
}
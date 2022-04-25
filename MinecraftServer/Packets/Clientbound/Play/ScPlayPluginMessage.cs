using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Play;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayPluginMessage : Packet<ScPlayPluginMessage, CsPlayPluginMessagePacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x18;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var p = Of(data);
        await stream.WriteStringAsync(p.Channel, ushort.MaxValue);
        await stream.WriteVarIntAsync(p.Data.Data.Count);
        await stream.WriteBytesAsync(p.Data);
    }
}
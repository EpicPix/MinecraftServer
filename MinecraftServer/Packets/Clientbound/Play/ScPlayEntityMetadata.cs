using MinecraftServer.EntityMetadata;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayEntityMetadata : Packet<ScPlayEntityMetadata, ScPlayEntityMetadataPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x4D;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var packet = Of(data);

        await stream.WriteVarInt(packet.EntityId);
        await Metadata.WriteUpdate(stream, packet.Metadata);
    }
}
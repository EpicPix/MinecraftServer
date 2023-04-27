using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlaySpawnPosition : Packet<ScPlaySpawnPosition, ScPlaySpawnPositionPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x50;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var p = Of(data);
        await stream.WritePositionAsync(p.Position);
        await stream.WriteFloatAsync(p.Angle);
    }
}
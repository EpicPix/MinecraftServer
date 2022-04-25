using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlaySpawnPlayer : Packet<ScPlaySpawnPlayer, ScPlaySpawnPlayerPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x04;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var packet = Of(data);

        await stream.WriteVarIntAsync(packet.EntityId);
        await stream.WriteUuidAsync(packet.Uuid);
        await stream.WriteDoubleAsync(packet.X);
        await stream.WriteDoubleAsync(packet.Y);
        await stream.WriteDoubleAsync(packet.Z);
        await stream.WriteAngleAsync(packet.Yaw);
        await stream.WriteAngleAsync(packet.Pitch);
    }
}
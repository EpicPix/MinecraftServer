using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayEntityTeleport : Packet<ScPlayEntityTeleport, ScPlayEntityTeleportPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x62;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var packet = Of(data);

        await stream.WriteVarIntAsync(packet.EntityId);
        await stream.WriteDoubleAsync(packet.X);
        await stream.WriteDoubleAsync(packet.Y);
        await stream.WriteDoubleAsync(packet.Z);
        await stream.WriteByteAsync(packet.Yaw);
        await stream.WriteByteAsync(packet.Pitch);
        await stream.WriteBoolAsync(packet.OnGround);
    }
}
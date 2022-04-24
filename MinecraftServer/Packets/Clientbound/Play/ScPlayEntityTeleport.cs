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

        await stream.WriteVarInt(packet.EntityId);
        await stream.WriteDouble(packet.X);
        await stream.WriteDouble(packet.Y);
        await stream.WriteDouble(packet.Z);
        await stream.WriteUByte(packet.Yaw);
        await stream.WriteUByte(packet.Pitch);
        await stream.WriteBool(packet.OnGround);
    }
}
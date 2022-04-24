using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayPlayerPositionAndRotation : Packet<ScPlayPlayerPositionAndRotation, ScPlayPlayerPositionAndRotationPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x2A;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var packet = Of(data);

        await stream.WriteVarInt(packet.EntityId);
        await stream.WriteUShort((ushort) packet.DeltaX);
        await stream.WriteUShort((ushort) packet.DeltaY);
        await stream.WriteUShort((ushort) packet.DeltaZ);
        await stream.WriteUByte(packet.Yaw);
        await stream.WriteUByte(packet.Pitch);
        await stream.WriteBool(packet.OnGround);
    }
}
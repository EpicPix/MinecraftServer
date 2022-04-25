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

        await stream.WriteVarIntAsync(packet.EntityId);
        await stream.WriteUnsignedShortAsync((ushort) packet.DeltaX);
        await stream.WriteUnsignedShortAsync((ushort) packet.DeltaY);
        await stream.WriteUnsignedShortAsync((ushort) packet.DeltaZ);
        await stream.WriteByteAsync(packet.Yaw);
        await stream.WriteByteAsync(packet.Pitch);
        await stream.WriteBoolAsync(packet.OnGround);
    }
}
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayPlayerPositionAndLook : Packet<ScPlayPlayerPositionAndLook, ScPlayPlayerPositionAndLookPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x38;

    public override ValueTask<PacketData> ReadPacket(NetworkConnection stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(NetworkConnection stream, PacketData data)
    {
        var packet = Of(data);
        
        await stream.WriteDouble(packet.X);
        await stream.WriteDouble(packet.Y);
        await stream.WriteDouble(packet.Z);
        await stream.WriteFloat(packet.Yaw);
        await stream.WriteFloat(packet.Pitch);
        await stream.WriteUByte((byte) packet.Flags);
        await stream.WriteVarInt(packet.TeleportId);
        await stream.WriteBool(packet.DismountVehicle);
    }
}
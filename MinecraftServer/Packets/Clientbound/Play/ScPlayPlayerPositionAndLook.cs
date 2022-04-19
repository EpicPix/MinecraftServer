using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayPlayerPositionAndLook : Packet<ScPlayPlayerPositionAndLook, ScPlayPlayerPositionAndLookPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x38;

    public override PacketData ReadPacket(NetworkConnection stream)
    {
        throw new NotImplementedException();
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        var packet = Of(data);
        
        stream.WriteDouble(packet.X);
        stream.WriteDouble(packet.Y);
        stream.WriteDouble(packet.Z);
        stream.WriteFloat(packet.Yaw);
        stream.WriteFloat(packet.Pitch);
        stream.WriteUByte((byte) packet.Flags);
        stream.WriteVarInt(packet.TeleportId);
        stream.WriteBool(packet.DismountVehicle);
    }
}
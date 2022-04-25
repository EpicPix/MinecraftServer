using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayPlayerPositionAndLook : Packet<ScPlayPlayerPositionAndLook, ScPlayPlayerPositionAndLookPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x38;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var packet = Of(data);
        
        await stream.WriteDoubleAsync(packet.X);
        await stream.WriteDoubleAsync(packet.Y);
        await stream.WriteDoubleAsync(packet.Z);
        await stream.WriteFloatAsync(packet.Yaw);
        await stream.WriteFloatAsync(packet.Pitch);
        await stream.WriteByteAsync((byte) packet.Flags);
        await stream.WriteVarIntAsync(packet.TeleportId);
        await stream.WriteBoolAsync(packet.DismountVehicle);
    }
}
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound;

public class ScLoginLoginSuccess : Packet<ScLoginLoginSuccess, ScLoginLoginSuccessPacketData>
{

    public override PacketType Type => PacketType.Login;
    public override PacketSide Side => PacketSide.Server;
    public override uint Id => 2;

    public override PacketData ReadPacket(NetworkConnection stream)
    {
        var uuid = stream.ReadUUID();
        var username = stream.ReadString(16);
        return new ScLoginLoginSuccessPacketData(uuid, username);
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        var packet = Of(data);
        stream.WriteUUID(packet.Uuid);
        stream.WriteString(packet.Username, 16);
    }
}
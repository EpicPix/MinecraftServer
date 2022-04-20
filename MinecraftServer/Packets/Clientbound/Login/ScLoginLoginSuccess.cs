using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Login;

public class ScLoginLoginSuccess : Packet<ScLoginLoginSuccess, ScLoginLoginSuccessPacketData>
{

    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 2;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var uuid = await stream.ReadUUID();
        var username = await stream.ReadString(16);
        return new ScLoginLoginSuccessPacketData(uuid, username);
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var packet = Of(data);
        await stream.WriteUUID(packet.Uuid);
        await stream.WriteString(packet.Username, 16);
    }
}
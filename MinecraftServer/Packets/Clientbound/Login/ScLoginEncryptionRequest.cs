using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Login;

public class ScLoginEncryptionRequest : Packet<ScLoginEncryptionRequest, ScLoginEncryptionRequestPacketData>
{
    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x01;
    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var serverId = await stream.ReadString(16);
        var pkl = await stream.ReadVarInt();
        var pubKey = new byte[pkl];
        await stream.ReadBytes(pubKey);
        var vtl = await stream.ReadVarInt();
        var verifyToken = new byte[vtl];
        await stream.ReadBytes(verifyToken);
        return new ScLoginEncryptionRequestPacketData(serverId, pubKey, verifyToken);
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var pkt = Of(data);
        await stream.WriteString(pkt.ServerId, 16);
        await stream.WriteBytesLen(pkt.PublicKey, 512);
        await stream.WriteBytesLen(pkt.VerifyToken, 4);
    }
}
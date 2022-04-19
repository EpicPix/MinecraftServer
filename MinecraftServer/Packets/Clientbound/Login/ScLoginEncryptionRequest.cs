using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Login;

public class ScLoginEncryptionRequest : Packet<ScLoginEncryptionRequest, ScLoginEncryptionRequestPacketData>
{
    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x01;
    public override PacketData ReadPacket(NetworkConnection stream)
    {
        var serverId = stream.ReadString(16);
        var pkl = stream.ReadVarInt();
        var pubKey = new byte[pkl];
        stream.ReadBytes(pubKey);
        var vtl = stream.ReadVarInt();
        var verifyToken = new byte[vtl];
        stream.ReadBytes(verifyToken);
        return new ScLoginEncryptionRequestPacketData(serverId, pubKey, verifyToken);
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        var pkt = Of(data);
        stream.WriteString(pkt.ServerId, 16);
        stream.WriteBytesLen(pkt.PublicKey, 512);
        stream.WriteBytesLen(pkt.VerifyToken, 4);
    }
}
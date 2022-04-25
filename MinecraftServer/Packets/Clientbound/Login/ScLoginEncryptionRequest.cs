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
        var serverId = await stream.ReadStringAsync(16);
        var pkl = await stream.ReadVarIntAsync();
        var pubKey = new byte[pkl];
        await stream.ReadBytesAsync(pubKey);
        var vtl = await stream.ReadVarIntAsync();
        var verifyToken = new byte[vtl];
        await stream.ReadBytesAsync(verifyToken);
        return new ScLoginEncryptionRequestPacketData(serverId, pubKey, verifyToken);
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var pkt = Of(data);
        await stream.WriteStringAsync(pkt.ServerId, 16);
        await stream.WriteBytesLenAsync(pkt.PublicKey, 512);
        await stream.WriteBytesLenAsync(pkt.VerifyToken, 4);
    }
}
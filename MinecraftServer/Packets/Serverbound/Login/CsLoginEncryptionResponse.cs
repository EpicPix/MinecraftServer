using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Login;

public class CsLoginEncryptionResponse : Packet<CsLoginEncryptionResponse, CsLoginEncryptionResponsePacketData>
{
    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x01;
    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        int sslen = await stream.ReadVarIntAsync();
        var ssbytes = new byte[sslen];
        await stream.ReadBytesAsync(ssbytes);
        int vtlen = await stream.ReadVarIntAsync();
        var vtbytes = new byte[vtlen];
        await stream.ReadBytesAsync(vtbytes);
        return new CsLoginEncryptionResponsePacketData(ssbytes, vtbytes);
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var pkt = Of(data);
        await stream.WriteBytesLenAsync(pkt.SharedSecret, 128);
        await stream.WriteBytesLenAsync(pkt.VerifyToken, 128);
    }
}
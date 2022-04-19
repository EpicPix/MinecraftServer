using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Login;

public class CsLoginEncryptionResponse : Packet<CsLoginEncryptionResponse, CsLoginEncryptionResponsePacketData>
{
    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x01;
    public override PacketData ReadPacket(NetworkConnection stream)
    {
        int sslen = stream.ReadVarInt();
        var ssbytes = new byte[sslen];
        stream.ReadBytes(ssbytes);
        int vtlen = stream.ReadVarInt();
        var vtbytes = new byte[vtlen];
        stream.ReadBytes(vtbytes);
        return new CsLoginEncryptionResponsePacketData(ssbytes, vtbytes);
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        var pkt = Of(data);
        stream.WriteBytesLen(pkt.SharedSecret, 128);
        stream.WriteBytesLen(pkt.VerifyToken, 128);
    }
}
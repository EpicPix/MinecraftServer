namespace MinecraftServer.Packets.Serverbound.Data;

public class CsLoginEncryptionResponsePacketData : PacketData
{
    public CsLoginEncryptionResponsePacketData(byte[] sharedSecret, byte[] verifyToken)
    {
        SharedSecret = sharedSecret;
        VerifyToken = verifyToken;
    }

    public int SharedSecretLength => SharedSecret.Length;
    public byte[] SharedSecret { get; }
    public int VerifyTokenLength => VerifyToken.Length;
    public byte[] VerifyToken { get; }
}
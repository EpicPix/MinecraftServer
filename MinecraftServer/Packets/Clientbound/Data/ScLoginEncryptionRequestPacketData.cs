namespace MinecraftServer.Packets.Clientbound.Data;

public class ScLoginEncryptionRequestPacketData : PacketData
{
    public ScLoginEncryptionRequestPacketData(string serverId, byte[] publicKey, byte[] verifyToken)
    {
        ServerId = serverId;
        PublicKey = publicKey;
        VerifyToken = verifyToken;
    }

    public string ServerId { get; }
    public int PublicKeyLength => PublicKey.Length;
    public byte[] PublicKey { get; }
    public int VerifyTokenLength => VerifyToken.Length;
    public byte[] VerifyToken { get; }
}
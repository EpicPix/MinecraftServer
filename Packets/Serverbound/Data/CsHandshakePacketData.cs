namespace MinecraftServer.Packets.Serverbound.Data;

public class CsHandshakePacketData : PacketData
{
    public int ProtocolVersion { get; }
    public string ServerIp { get; }
    public ushort ServerPort { get; }
    public int NextState { get; }
        
    public CsHandshakePacketData(int protocolVersion, string serverIp, ushort serverPort, int nextState)
    {
        ProtocolVersion = protocolVersion;
        ServerIp = serverIp;
        ServerPort = serverPort;
        NextState = nextState;
    }
}
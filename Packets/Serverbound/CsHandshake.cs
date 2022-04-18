namespace MinecraftServer.Packets.Serverbound;

public class CsHandshake : Packet<CsHandshakePacketData, CsHandshake>
{
    public override PacketType Type => PacketType.Handshake;
    public override PacketSide Side => PacketSide.Client;
    public override uint Id => 0;

    public override async Task<CsHandshakePacketData> ReadPacket(NetworkConnection connection)
    {
        var protocolVersion = await connection.ReadVarInt();
        var serverIp = await connection.ReadString(255);
        var serverPort = await connection.ReadUShort();
        var nextState = await connection.ReadVarInt();
        return new CsHandshakePacketData(protocolVersion, serverIp, serverPort, nextState);
    }

    public override async Task WritePacket(NetworkConnection connection, CsHandshakePacketData data)
    {
        await connection.WriteVarInt(data.ProtocolVersion);
        await connection.WriteString(data.ServerIp, 255);
        await connection.WriteUShort(data.ServerPort);
        await connection.WriteVarInt(data.NextState);
    }
    
}

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
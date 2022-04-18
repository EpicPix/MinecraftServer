namespace MinecraftServer.Packets;

public class HandshakeClient : Packet<HandshakePacketData>
{
    PacketType Packet.Type => PacketType.Handshake;
    PacketSide Packet.Side => PacketSide.Client;
    uint Packet.Id => 0;

    public async Task<HandshakePacketData> ReadPacket(NetworkConnection connection)
    {
        var protocolVersion = await connection.ReadVarInt();
        var serverIp = await connection.ReadString(255);
        var serverPort = await connection.ReadUShort();
        var nextState = await connection.ReadVarInt();
        return new HandshakePacketData(protocolVersion, serverIp, serverPort, nextState);
    }

    public async Task WritePacket(NetworkConnection connection, HandshakePacketData data)
    {
        await connection.WriteVarInt(data.ProtocolVersion);
        await connection.WriteString(data.ServerIp, 255);
        await connection.WriteUShort(data.ServerPort);
        await connection.WriteVarInt(data.NextState);
    }
    
}

public class HandshakePacketData : PacketData
{
        
    public int ProtocolVersion { get; }
    public string ServerIp { get; }
    public ushort ServerPort { get; }
    public int NextState { get; }
        
    public HandshakePacketData(int protocolVersion, string serverIp, ushort serverPort, int nextState)
    {
        ProtocolVersion = protocolVersion;
        ServerIp = serverIp;
        ServerPort = serverPort;
        NextState = nextState;
    }

}
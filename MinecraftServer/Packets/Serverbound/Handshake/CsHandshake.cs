using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Handshake;

public class CsHandshake : Packet<CsHandshake, CsHandshakePacketData>
{
    public override PacketType Type => PacketType.Handshake;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0;

    public override PacketData ReadPacket(NetworkConnection connection)
    {
        var protocolVersion = connection.ReadVarInt();
        var serverIp = connection.ReadString(255);
        var serverPort = connection.ReadUShort();
        var nextState = connection.ReadVarInt();
        return new CsHandshakePacketData(protocolVersion, serverIp, serverPort, nextState);
    }

    public override void WritePacket(NetworkConnection connection, PacketData input)
    {
        var data = Of(input);
        connection.WriteVarInt(data.ProtocolVersion);
        connection.WriteString(data.ServerIp, 255);
        connection.WriteUShort(data.ServerPort);
        connection.WriteVarInt(data.NextState);
    }
    
}
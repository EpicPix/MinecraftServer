using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Handshake;

public class CsHandshake : Packet<CsHandshake, CsHandshakePacketData>
{
    public override PacketType Type => PacketType.Handshake;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0;

    public override async ValueTask<PacketData> ReadPacket(NetworkConnection connection)
    {
        var protocolVersion = await connection.ReadVarInt();
        var serverIp = await connection.ReadString(255);
        var serverPort = await connection.ReadUShort();
        var nextState = await connection.ReadVarInt();
        return new CsHandshakePacketData(protocolVersion, serverIp, serverPort, nextState);
    }

    public override async ValueTask WritePacket(NetworkConnection connection, PacketData input)
    {
        var data = Of(input);
        await connection.WriteVarInt(data.ProtocolVersion);
        await connection.WriteString(data.ServerIp, 255);
        await connection.WriteUShort(data.ServerPort);
        await connection.WriteVarInt(data.NextState);
    }
    
}
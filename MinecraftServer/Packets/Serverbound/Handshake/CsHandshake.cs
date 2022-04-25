using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Handshake;

public class CsHandshake : Packet<CsHandshake, CsHandshakePacketData>
{
    public override PacketType Type => PacketType.Handshake;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter connection)
    {
        var protocolVersion = await connection.ReadVarIntAsync();
        var serverIp = await connection.ReadStringAsync(255);
        var serverPort = await connection.ReadUnsignedShortAsync();
        var nextState = await connection.ReadVarIntAsync();
        return new CsHandshakePacketData(protocolVersion, serverIp, serverPort, nextState);
    }

    public override async ValueTask WritePacket(DataAdapter connection, PacketData input)
    {
        var data = Of(input);
        await connection.WriteVarIntAsync(data.ProtocolVersion);
        await connection.WriteStringAsync(data.ServerIp, 255);
        await connection.WriteUnsignedShortAsync(data.ServerPort);
        await connection.WriteVarIntAsync(data.NextState);
    }
    
}
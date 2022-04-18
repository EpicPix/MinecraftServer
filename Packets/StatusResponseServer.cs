using System.Text.Json;

namespace MinecraftServer.Packets;

public class StatusResponseServer : Packet<StatusResponsePacketData>
{

    PacketType Packet.Type => PacketType.Status;
    PacketSide Packet.Side => PacketSide.Server;
    uint Packet.Id => 0;

    public async Task<StatusResponsePacketData> ReadPacket(NetworkConnection stream)
    {
        var info = await stream.ReadString(32768);
        return new StatusResponsePacketData(JsonSerializer.Deserialize<ServerInfo>(info));
    }

    public async Task WritePacket(NetworkConnection stream, StatusResponsePacketData data)
    {
        await stream.WriteString(JsonSerializer.Serialize(data.ServerInfo), 32768);
    }
}

public class StatusResponsePacketData : PacketData
{
    public ServerInfo ServerInfo { get; }

    public StatusResponsePacketData(ServerInfo serverInfo)
    {
        ServerInfo = serverInfo;
    }
}
using System.Text.Json;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound;

public class ScStatusResponse : Packet<ScStatusResponsePacketData, ScStatusResponse>
{
    public override PacketType Type => PacketType.Status;
    public override PacketSide Side => PacketSide.Server;
    public override uint Id => 0;

    public override async Task<ScStatusResponsePacketData> ReadPacket(NetworkConnection stream)
    {
        var info = await stream.ReadString(32768);
        return new ScStatusResponsePacketData(JsonSerializer.Deserialize<ServerInfo>(info));
    }

    public override async Task WritePacket(NetworkConnection stream, ScStatusResponsePacketData data)
    {
        await stream.WriteString(JsonSerializer.Serialize(data.ServerInfo), 32768);
    }
}
using System.Text.Json;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound;

public class ScStatusResponse : Packet<ScStatusResponse, ScStatusResponsePacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketSide Side => PacketSide.Server;
    public override uint Id => 0;

    public override PacketData ReadPacket(NetworkConnection stream)
    {
        var info = stream.ReadString(32768);
        return new ScStatusResponsePacketData(JsonSerializer.Deserialize<ServerInfo>(info));
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        stream.WriteString(JsonSerializer.Serialize(Of(data).ServerInfo), 32768);
    }
}
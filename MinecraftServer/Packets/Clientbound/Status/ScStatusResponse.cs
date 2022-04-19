using System.Text.Json;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Status;

public class ScStatusResponse : Packet<ScStatusResponse, ScStatusResponsePacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0;

    public override PacketData ReadPacket(NetworkConnection stream)
    {
        var info = stream.ReadString(32768);
        return new ScStatusResponsePacketData(JsonSerializer.Deserialize<ServerInfo>(info));
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        
        using var mem = new MemoryStream();
        JsonSerializer.Serialize(mem, Of(data).ServerInfo);
        stream.WriteBytesLen(mem.GetBuffer(), 32768);
    }
}
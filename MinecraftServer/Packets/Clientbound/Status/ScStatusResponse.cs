using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Status;

public class ScStatusResponse : Packet<ScStatusResponse, ScStatusResponsePacketData>
{
    public override PacketType Type => PacketType.Status;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var info = await stream.ReadStringAsync(32768);
        return new ScStatusResponsePacketData(JsonSerializer.Deserialize(info, SerializationContext.Default.ServerInfo!));
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        
        using var mem = new MemoryStream();
        JsonSerializer.Serialize(mem, Of(data).ServerInfo, SerializationContext.Default.ServerInfo!);
        await stream.WriteBytesLenAsync(mem.GetBuffer(), 32768);
    }
}
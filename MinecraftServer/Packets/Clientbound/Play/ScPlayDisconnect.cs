using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayDisconnect : Packet<ScPlayDisconnect, ScDisconnectPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x1A;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var chatComponent = JsonSerializer.Deserialize(await stream.ReadStringAsync(ushort.MaxValue), SerializationContext.Default.ChatComponent);
        if (chatComponent == null)
        {
            throw new NullReferenceException("Deserializing returned null");
        }
        return new ScDisconnectPacketData(chatComponent);
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        await using var mem = new MemoryStream();
        await JsonSerializer.SerializeAsync(mem, Of(data).Reason, SerializationContext.Default.ChatComponent);
        await stream.WriteBytesLenAsync(mem.ToArray(), ushort.MaxValue);
    }
}
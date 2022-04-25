using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Login;

public class ScLoginDisconnect : Packet<ScLoginDisconnect, ScDisconnectPacketData>
{

    public override PacketType Type => PacketType.Login;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0;

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
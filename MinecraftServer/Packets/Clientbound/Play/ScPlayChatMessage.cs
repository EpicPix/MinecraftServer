using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayChatMessage : Packet<ScPlayChatMessage, ScPlayChatMessagePacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x1B;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var chatComponent = JsonSerializer.Deserialize(await stream.ReadStringAsync(ushort.MaxValue), SerializationContext.Default.ChatComponent);
        if (chatComponent == null)
        {
            throw new NullReferenceException("Deserializing returned null");
        }
        var position = (ScPlayChatMessagePacketData.PositionType) await stream.ReadByteAsync();
        var sender = await stream.ReadUuidAsync();
        return new ScPlayChatMessagePacketData(chatComponent, position, sender);
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var packet = Of(data);
        
        await using var mem = new MemoryStream();
        await JsonSerializer.SerializeAsync(mem, packet.Data, SerializationContext.Default.ChatComponent);
        await stream.WriteBytesLenAsync(mem.ToArray(), ushort.MaxValue);
        await stream.WriteVarIntAsync(0);
        await stream.WriteStringAsync("{\"text\":\"the component\"}}", ushort.MaxValue);
        await stream.WriteBoolAsync(false);
    }
}
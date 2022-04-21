using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayChatMessage : Packet<ScPlayChatMessage, ScPlayChatMessagePacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x0F;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var chatComponent = JsonSerializer.Deserialize(await stream.ReadString(ushort.MaxValue), SerializationContext.Default.ChatComponent);
        if (chatComponent == null)
        {
            throw new NullReferenceException("Deserializing returned null");
        }
        var position = (ScPlayChatMessagePacketData.PositionType) await stream.ReadUByte();
        var sender = await stream.ReadUUID();
        return new ScPlayChatMessagePacketData(chatComponent, position, sender);
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var packet = Of(data);
        
        await using var mem = new MemoryStream();
        await JsonSerializer.SerializeAsync(mem, packet.Data, SerializationContext.Default.ChatComponent);
        await stream.WriteBytesLen(mem.ToArray(), ushort.MaxValue);

        await stream.WriteUByte((byte) packet.Position);
        await stream.WriteUUID(packet.Sender);
    }
}
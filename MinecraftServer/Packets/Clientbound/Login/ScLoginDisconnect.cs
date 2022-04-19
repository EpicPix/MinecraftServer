using System.Text.Json;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Login;

public class ScLoginDisconnect : Packet<ScLoginDisconnect, ScLoginDisconnectPacketData>
{

    public override PacketType Type => PacketType.Login;
    public override PacketSide Side => PacketSide.Server;
    public override uint Id => 0;

    public override PacketData ReadPacket(NetworkConnection stream)
    {
        var chatComponent = JsonSerializer.Deserialize<ChatComponent>(stream.ReadString(ushort.MaxValue));
        if (chatComponent == null)
        {
            throw new NullReferenceException("Deserializing returned null");
        }
        return new ScLoginDisconnectPacketData(chatComponent);
    }

    public override void WritePacket(NetworkConnection stream, PacketData data)
    {
        using var mem = new MemoryStream();
        JsonSerializer.Serialize(mem, Of(data).Reason);
        stream.WriteBytesLen(mem.GetBuffer(), ushort.MaxValue);
    }
}
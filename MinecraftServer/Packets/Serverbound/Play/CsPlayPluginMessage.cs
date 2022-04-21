using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayPluginMessage : Packet<CsPlayPluginMessage, CsPlayPluginMessagePacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x0A;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        if (stream is not NetworkConnection con) throw new ArgumentException($"Expected {typeof(NetworkConnection)} but got {stream.GetType()}");
        
        var identifier = await stream.ReadString(ushort.MaxValue);
        var bytesLength = con.PacketDataLength - Utils.GetVarIntLength(identifier.Length) - identifier.Length;
        byte[] bytes = new byte[bytesLength];
        await stream.ReadBytes(bytes);
        return new CsPlayPluginMessagePacketData(identifier, bytes);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }
}
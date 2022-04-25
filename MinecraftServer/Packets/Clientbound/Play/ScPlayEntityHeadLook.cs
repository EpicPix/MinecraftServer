using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayEntityHeadLook : Packet<ScPlayEntityHeadLook, ScPlayEntityHeadLookPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x3E;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        var packet = Of(data);

        await stream.WriteVarIntAsync(packet.EntityId);
        await stream.WriteAngleAsync(packet.HeadYaw);
    }
}
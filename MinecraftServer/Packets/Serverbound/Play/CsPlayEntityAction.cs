using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets.Serverbound.Play;

public class CsPlayEntityAction : Packet<CsPlayEntityAction, CsPlayEntityActionPacketData>
{

    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Server;
    public override uint Id => 0x1B;

    public override async ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        var entityId = await stream.ReadVarIntAsync();
        var action = (CsPlayEntityActionPacketData.ActionType) await stream.ReadVarIntAsync();
        var jumpBoost = await stream.ReadVarIntAsync();
        return new CsPlayEntityActionPacketData(entityId, action, jumpBoost);
    }

    public override ValueTask WritePacket(DataAdapter stream, PacketData data)
    {
        throw new NotImplementedException();
    }

}
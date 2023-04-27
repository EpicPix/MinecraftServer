using System.Text.Json;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;

namespace MinecraftServer.Packets.Clientbound.Play;

public class ScPlayPlayerInfoRemove : Packet<ScPlayPlayerInfoRemove, ScPlayPlayerInfoRemovePacketData>
{
    
    public override PacketType Type => PacketType.Play;
    public override PacketBound Bound => PacketBound.Client;
    public override uint Id => 0x39;

    public override ValueTask<PacketData> ReadPacket(DataAdapter stream)
    {
        throw new NotImplementedException();
    }

    public override async ValueTask WritePacket(DataAdapter stream, PacketData pData)
    {
        var data = Of(pData);

        await stream.WriteVarIntAsync(data.PlayerIds.Count);
        foreach (var playerId in data.PlayerIds)
        {
            await stream.WriteUuidAsync(playerId);
        }
    }
}
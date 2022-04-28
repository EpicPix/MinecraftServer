using MinecraftServer.Data;
using MinecraftServer.Types;

namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayBlockChangePacketData : PacketData
{

    public Position Position { get; }
    public BlockState BlockState { get; }

    public ScPlayBlockChangePacketData(Position position, BlockState blockState)
    {
        Position = position;
        BlockState = blockState;
    }

}
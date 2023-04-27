using MinecraftServer.Types;

namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlaySpawnPositionPacketData : PacketData
{
    public Position Position { get; }
    public float Angle { get; }

    public ScPlaySpawnPositionPacketData(Position position, float a)
    {
        Position = position;
        Angle = a;
    }

    public override string ToString()
    {
        return $"ScPlaySpawnPositionPacketData[Position={Position},Angle={Angle}]";
    }

}
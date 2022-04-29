using MinecraftServer.Types;

namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayPlayerDiggingPacketData : PacketData
{

    public enum DiggingStatus
    {
        StartedDigging = 0,
        CancelledDigging = 1,
        FinishedDigging = 2,
        DropItemStack = 3,
        DropItem = 4,
        FinishAction = 5, // also Shoot Arrow / Finish Eating
        SwapItemInHand = 6
    }
    
    public DiggingStatus Status { get; }
    public Position Position { get; }
    public byte Face { get; }

    public CsPlayPlayerDiggingPacketData(DiggingStatus status, Position position, byte face)
    {
        Status = status;
        Position = position;
        Face = face;
    }

    public override string ToString()
    {
        return $"CsPlayPlayerDiggingPacketData[Status={Status},Position={Position},Face={Face}]";
    }
}
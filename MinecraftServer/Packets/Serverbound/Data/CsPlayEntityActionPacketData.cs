namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayEntityActionPacketData : PacketData
{

    public enum ActionType {
        StartSneaking = 0,
        StopSneaking = 1,
        LeaveBed = 2,
        StartSprinting = 3,
        StopSprinting = 4,
        StartJumpWithHorse = 5,
        StopJumpWithHorse = 6,
        OpenHorseInventory = 7,
        StartFlyingWithElytra = 8
    }

    public int EntityId { get; }
    public ActionType Action { get; }
    public int JumpBoost { get; }

    public CsPlayEntityActionPacketData(int entityId, ActionType action, int jumpBoost)
    {
        EntityId = entityId;
        Action = action;
        JumpBoost = jumpBoost;
    }

    public override string ToString()
    {
        return $"CsPlayEntityActionPacketData[EntityId={EntityId},Action={Action},JumpBoost={JumpBoost}]";
    }

}
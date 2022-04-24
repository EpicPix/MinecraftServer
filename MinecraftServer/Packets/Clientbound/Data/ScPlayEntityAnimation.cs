using System.Text;

namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayerEntityAnimationPacketData : PacketData
{

    public enum AnimationType
    {
        SwingMainArm = 0,
        TakeDamage = 1,
        LeaveBed = 2,
        SwingOffhand = 3,
        CriticalEffect = 4,
        MagicCriticalEffect = 5
    }
    
    public int EntityId { get; }
    public AnimationType Animation { get; }

    public ScPlayerEntityAnimationPacketData(int entityId, AnimationType animation)
    {
        EntityId = entityId;
        Animation = animation;
    }

    public override string ToString()
    {
        return $"ScPlayerEntityAnimationPacketData[EntityId={EntityId},Animation={Animation}]";
    }

}
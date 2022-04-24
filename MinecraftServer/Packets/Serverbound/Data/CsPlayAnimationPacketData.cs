namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayAnimationPacketData : PacketData
{

    public enum UsedHand
    {
        MainHand = 0,
        OffHand = 1
    }

    public UsedHand Hand;

    public CsPlayAnimationPacketData(UsedHand hand)
    {
        Hand = hand;
    }

}
namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayUpdateViewPositionPacketData : PacketData
{
    public int X { get; }
    public int Z { get; }

    public ScPlayUpdateViewPositionPacketData(int x, int z)
    {
        X = x;
        Z = z;
    }

    public override string ToString()
    {
        return $"ScPlayUpdateViewPositionPacketData[X={X},Z={Z}]";
    }

}
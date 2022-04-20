using System.Text;

namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayPlayerPositionPacketData : PacketData
{

    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public bool OnGround { get; }

    public CsPlayPlayerPositionPacketData(double x, double y, double z, bool onGround)
    {
        X = x;
        Y = y;
        Z = z;
        OnGround = onGround;
    }

    public override string ToString()
    {
        return $"CsPlayPlayerPositionPacketData[X={X},Y={Y},Z={Z},OnGround={OnGround}]";
    }

}
using System.Text;

namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayPlayerPositionAndRotationPacketData : PacketData
{

    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public float Yaw { get; }
    public float Pitch { get; }
    public bool OnGround { get; }

    public CsPlayPlayerPositionAndRotationPacketData(double x, double y, double z, float yaw, float pitch, bool onGround)
    {
        X = x;
        Y = y;
        Z = z;
        Yaw = yaw;
        Pitch = pitch;
        OnGround = onGround;
    }

    public override string ToString()
    {
        return $"CsPlayPlayerPositionAndRotationPacketData[X={X},Y={Y},Z={Z},Yaw={Yaw},Pitch={Pitch},OnGround={OnGround}]";
    }

}
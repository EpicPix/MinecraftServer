using System.Text;

namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayPlayerRotationPacketData : PacketData
{

    public float Yaw { get; }
    public float Pitch { get; }
    public bool OnGround { get; }

    public CsPlayPlayerRotationPacketData(float yaw, float pitch, bool onGround)
    {
        Yaw = yaw;
        Pitch = pitch;
        OnGround = onGround;
    }

    public override string ToString()
    {
        return $"CsPlayPlayerRotationPacketData[Yaw={Yaw},Pitch={Pitch},OnGround={OnGround}]";
    }

}
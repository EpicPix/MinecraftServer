namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayPlayerPositionAndLookPacketData : PacketData
{
    public enum RelativeFlags
    {
        X = 0x01,
        Y = 0x02,
        Z = 0x04,
        YRot = 0x08,
        XRot = 0x10
    }
    
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    
    public float Yaw { get; }
    public float Pitch { get; }
    
    public RelativeFlags Flags { get; }

    public int TeleportId { get; }

    public bool DismountVehicle { get; }

    public ScPlayPlayerPositionAndLookPacketData(double x, double y, double z, float yaw, float pitch, RelativeFlags flags, int teleportId, bool dismountVehicle)
    {
        X = x;
        Y = y;
        Z = z;
        Yaw = yaw;
        Pitch = pitch;
        Flags = flags;
        TeleportId = teleportId;
        DismountVehicle = dismountVehicle;
    }

}
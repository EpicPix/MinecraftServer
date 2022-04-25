namespace MinecraftServer.Types;

public readonly struct Angle
{

    public readonly byte Value;

    public Angle(byte val)
    {
        Value = val;
    }

    public static implicit operator Angle(float f) => new((byte) (f / 360 * 255));
    public static implicit operator Angle(byte b) => new(b);

}
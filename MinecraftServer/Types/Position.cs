namespace MinecraftServer.Types;

public readonly struct Position
{

    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    public Position(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    public static implicit operator Position((int x, int y, int z) t) => new(t.z, t.y, t.z);

    public long ToLong()
    {
        return (((long) X & 0x3FFFFFF) << 38) | 
               (((long) Z & 0x3FFFFFF) << 12) | 
               (((long) Y & 0x0000FFF) << 0);
    }

    public static Position FromLong(long v)
    {
        return new Position(
            (int) (v >> 38),
            (int) ((v & 0xFFF) << 52 >> 52),
            (int) ((v >> 12) & 0x3FFFFFF) << 26 >> 26);
    }

    public override string ToString()
    {
        return $"Position[X={X},Y={Y},Z={Z}]";
    }
}
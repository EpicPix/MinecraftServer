namespace MinecraftServer.Nbt;

public class NbtTagDouble : NbtTag
{
    public byte Id => 6;

    public double Double { get; }

    public NbtTagDouble(double d)
    {
        Double = d;
    }

    public void Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public void Write(NetworkConnection writer)
    {
        writer.WriteDouble(Double);
    }
}
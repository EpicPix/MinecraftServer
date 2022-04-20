namespace MinecraftServer.Nbt;

public class NbtTagDouble : NbtTag
{
    public byte Id => 6;

    public double Double { get; }

    public NbtTagDouble(double d)
    {
        Double = d;
    }

    public ValueTask Read(DataAdapter reader)
    {
        throw new NotImplementedException();
    }

    public ValueTask Write(DataAdapter writer)
    {
        return writer.WriteDouble(Double);
    }
}
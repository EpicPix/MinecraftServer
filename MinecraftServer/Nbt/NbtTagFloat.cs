namespace MinecraftServer.Nbt;

public class NbtTagFloat : NbtTag
{
    public byte Id => 5;

    public float Float { get; }

    public NbtTagFloat(float f)
    {
        Float = f;
    }

    public ValueTask Read(DataAdapter reader)
    {
        throw new NotImplementedException();
    }

    public ValueTask Write(DataAdapter writer)
    {
        return writer.WriteFloat(Float);
    }
}
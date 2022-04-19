namespace MinecraftServer.Nbt;

public class NbtTagFloat : NbtTag
{
    public byte Id => 5;

    public float Float { get; }

    public NbtTagFloat(float f)
    {
        Float = f;
    }

    public ValueTask Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public ValueTask Write(NetworkConnection writer)
    {
        return writer.WriteFloat(Float);
    }
}
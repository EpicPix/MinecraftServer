namespace MinecraftServer.Nbt;

public class NbtTagFloat : NbtTag
{
    public byte Id => 5;

    public float Float { get; }

    public NbtTagFloat(float f)
    {
        Float = f;
    }

    public void Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public void Write(NetworkConnection writer)
    {
        writer.WriteFloat(Float);
    }
}
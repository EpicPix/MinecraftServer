namespace MinecraftServer.Nbt;

public class NbtTagInteger : NbtTag
{
    public byte Id => 3;

    public int Integer { get; }

    public NbtTagInteger(int i)
    {
        Integer = i;
    }

    public void Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public void Write(NetworkConnection writer)
    {
        writer.WriteInt(Integer);
    }
}
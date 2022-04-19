namespace MinecraftServer.Nbt;

public class NbtTagInteger : NbtTag
{
    public byte Id => 3;

    public int Integer { get; }

    public NbtTagInteger(int i)
    {
        Integer = i;
    }

    public ValueTask Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public ValueTask Write(NetworkConnection writer)
    {
        return writer.WriteInt(Integer);
    }
}
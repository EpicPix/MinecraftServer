namespace MinecraftServer.Nbt;

public class NbtTagByte : NbtTag
{
    public byte Id => 1;

    public sbyte Byte { get; }

    public NbtTagByte(sbyte b)
    {
        Byte = b;
    }

    public ValueTask Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public ValueTask Write(NetworkConnection writer)
    {
        return writer.WriteUByte((byte) Byte);
    }
}
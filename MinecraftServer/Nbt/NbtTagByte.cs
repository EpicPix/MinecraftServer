namespace MinecraftServer.Nbt;

public class NbtTagByte : NbtTag
{
    public byte Id => 3;

    public sbyte Byte { get; }

    public NbtTagByte(sbyte b)
    {
        Byte = b;
    }

    public void Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public void Write(NetworkConnection writer)
    {
        writer.WriteUByte((byte) Byte);
    }
}
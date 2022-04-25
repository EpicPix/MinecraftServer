using MinecraftServer.Networking;

namespace MinecraftServer.Nbt;

public class NbtTagByte : NbtTag
{
    public byte Id => 1;

    public sbyte Byte { get; }

    public NbtTagByte(sbyte b)
    {
        Byte = b;
    }

    public ValueTask Read(DataAdapter reader)
    {
        throw new NotImplementedException();
    }

    public ValueTask Write(DataAdapter writer)
    {
        return writer.WriteByteAsync((byte) Byte);
    }
}
using MinecraftServer.Networking;

namespace MinecraftServer.Nbt;

public class NbtTagLongArray : NbtTag
{
    
    public byte Id => 12;
    private List<long> longs = new();

    public ValueTask Read(DataAdapter reader)
    {
        throw new NotImplementedException();
    }

    public async ValueTask Write(DataAdapter writer)
    {
        await writer.WriteInt(longs.Count);
        foreach (var value in longs)
        {
            await writer.WriteULong((ulong) value);
        }
    }

    public NbtTagLongArray Add(long value)
    {
        longs.Add(value);
        return this;
    }

    public NbtTagLongArray Remove(long value)
    {
        longs.Remove(value);
        return this;
    }

}
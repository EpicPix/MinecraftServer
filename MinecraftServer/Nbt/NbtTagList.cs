using System.Diagnostics.Tracing;

namespace MinecraftServer.Nbt;

public class NbtTagList<T> : NbtTag where T : NbtTag
{
    public byte Id => 9;
    private List<T> tags = new();

    public ValueTask Read(DataAdapter reader)
    {
        throw new NotImplementedException();
    }

    public async ValueTask Write(DataAdapter writer)
    {
        await writer.WriteUByte(NbtTag.GetTag<T>());
        await writer.WriteInt(tags.Count);
        foreach (var value in tags)
        {
            await value.Write(writer);
        }
    }

    public NbtTagList<T> Add(T value)
    {
        tags.Add(value);
        return this;
    }

    public NbtTagList<T> Remove(T value)
    {
        tags.Remove(value);
        return this;
    }
    
}
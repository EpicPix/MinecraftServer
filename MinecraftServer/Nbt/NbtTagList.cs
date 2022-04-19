using System.Diagnostics.Tracing;

namespace MinecraftServer.Nbt;

public class NbtTagList<T> : NbtTag where T : NbtTag
{
    public byte Id => 9;
    private List<T> tags = new();

    public void Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public void Write(NetworkConnection writer)
    {
        writer.WriteUByte(NbtTag.GetTag<T>());
        writer.WriteInt(tags.Count);
        foreach (var value in tags)
        {
            value.Write(writer);
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
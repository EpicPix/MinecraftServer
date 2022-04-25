using System.Diagnostics.Tracing;
using MinecraftServer.Networking;

namespace MinecraftServer.Nbt;

public class NbtTagCompound : NbtTag
{
    public byte Id => 10;
    private Dictionary<string, NbtTag?> tags = new();

    public ValueTask Read(DataAdapter reader)
    {
        throw new NotImplementedException();
    }

    public virtual async ValueTask Write(DataAdapter writer)
    {
        foreach (var (key, value) in tags)
        {
            if (value == null) throw new NullReferenceException("Unexpected null");
            await writer.WriteByteAsync(value.Id);
            await writer.WriteStringShortAsync(key);
            await value.Write(writer);
        }
        await writer.WriteByteAsync(0);
    }

    public virtual NbtTagCompound Set(string key, NbtTag? value)
    {
        if (value != null)
        {
            tags[key] = value;
        } else
        {
            tags.Remove(key);
        }
        return this;
    }

    public virtual NbtTagCompound SetString(string key, string value)
    {
        return Set(key, new NbtTagString(value));
    }

    public virtual NbtTagCompound SetByte(string key, sbyte value)
    {
        return Set(key, new NbtTagByte(value));
    }

    public virtual NbtTagCompound SetInteger(string key, int value)
    {
        return Set(key, new NbtTagInteger(value));
    }

    public virtual NbtTagCompound SetFloat(string key, float value)
    {
        return Set(key, new NbtTagFloat(value));
    }

    public virtual NbtTagCompound SetDouble(string key, double value)
    {
        return Set(key, new NbtTagDouble(value));
    }

    public virtual NbtTagCompound Remove(string key)
    {
        tags.Remove(key);
        return this;
    }

    public NbtTag? Get(string key)
    {
        return tags[key];
    }

    public NbtTag? this[string key] {
        get => tags[key];
        set {
            if (value == null)
            {
                tags.Remove(key);
            } else
            {
                tags[key] = value;
            }
        }
    }
    
}
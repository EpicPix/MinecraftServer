using System.Diagnostics.Tracing;

namespace MinecraftServer.Nbt;

public class NbtTagCompound : NbtTag
{
    public byte Id => 10;
    private Dictionary<string, NbtTag?> tags = new();

    public void Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public virtual void Write(NetworkConnection writer)
    {
        foreach (var (key, value) in tags)
        {
            if (value == null) throw new NullReferenceException("Unexpected null");
            writer.WriteUByte(value.Id);
            writer.WriteStringShort(key);
            value.Write(writer);
        }
        writer.WriteUByte(0);
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
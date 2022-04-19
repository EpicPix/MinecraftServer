using System.Diagnostics.Tracing;

namespace MinecraftServer.Nbt;

public class NbtTagRoot : NbtTagCompound
{
    public override async ValueTask Write(NetworkConnection writer)
    {
        await writer.WriteUByte(Id);
        await writer.WriteStringShort("");
        await base.Write(writer);
    }

    public override NbtTagRoot Set(string key, NbtTag? value)
    {
        return (NbtTagRoot) base.Set(key, value);
    }

    public override NbtTagRoot SetString(string key, string value)
    {
        return Set(key, new NbtTagString(value));
    }

    public override NbtTagRoot SetByte(string key, sbyte value)
    {
        return Set(key, new NbtTagByte(value));
    }

    public override NbtTagRoot SetInteger(string key, int value)
    {
        return Set(key, new NbtTagInteger(value));
    }

    public override NbtTagRoot SetFloat(string key, float value)
    {
        return Set(key, new NbtTagFloat(value));
    }

    public override NbtTagRoot SetDouble(string key, double value)
    {
        return Set(key, new NbtTagDouble(value));
    }

    public override NbtTagRoot Remove(string key)
    {
        return (NbtTagRoot) base.Remove(key);;
    }
    
}
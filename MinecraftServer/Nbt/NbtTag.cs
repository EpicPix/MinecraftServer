namespace MinecraftServer.Nbt;

public interface NbtTag
{
    public byte Id { get; }

    public static byte GetTag<T>() where T : NbtTag
    {
        if (typeof(T).IsAssignableTo(typeof(NbtTagCompound)))
        {
            return 10;
        }
        if (typeof(T).IsAssignableTo(typeof(NbtTagList<>)))
        {
            return 9;
        }
        if (typeof(T).IsAssignableTo(typeof(NbtTagString)))
        {
            return 8;
        }
        if (typeof(T).IsAssignableTo(typeof(NbtTagDouble)))
        {
            return 6;
        }
        if (typeof(T).IsAssignableTo(typeof(NbtTagFloat)))
        {
            return 5;
        }
        if (typeof(T).IsAssignableTo(typeof(NbtTagInteger)))
        {
            return 3;
        }
        
        throw new NotSupportedException($"Nbt Type {typeof(T)} is not supported");
    }

    public static NbtTag ReadTag(NetworkConnection reader)
    {
        byte tag = reader.ReadUByte();

        throw new NotSupportedException($"Nbt Tag {tag} is not supported");
    }
    
    public void Read(NetworkConnection reader);
    public void Write(NetworkConnection writer);
}
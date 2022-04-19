namespace MinecraftServer.Nbt;

public class NbtTagString : NbtTag
{

    public byte Id => 8;

    private string _string;

    public string String => _string;

    public NbtTagString() { }

    public NbtTagString(string str)
    {
        _string = str;
    }

    public ValueTask Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public ValueTask Write(NetworkConnection writer)
    {
        return writer.WriteStringShort(_string);
    }
}
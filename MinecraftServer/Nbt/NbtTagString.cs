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

    public void Read(NetworkConnection reader)
    {
        throw new NotImplementedException();
    }

    public void Write(NetworkConnection writer)
    {
        writer.WriteStringShort(_string);
    }
}
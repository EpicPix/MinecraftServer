using MinecraftServer.Networking;

namespace MinecraftServer.EntityMetadata;

public class MetadataByte : IMetadataValue
{

    public int Type => 0x00;

    public byte Value { get; }

    public MetadataByte(byte value)
    {
        Value = value;
    }

    public async ValueTask Write(DataAdapter writer)
    {
        await writer.WriteByteAsync(Value);
    }
}
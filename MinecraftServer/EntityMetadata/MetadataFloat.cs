using MinecraftServer.Networking;

namespace MinecraftServer.EntityMetadata;

public class MetadataFloat : IMetadataValue
{

    public int Type => 0x02;

    public float Value { get; }

    public MetadataFloat(float value)
    {
        Value = value;
    }

    public async ValueTask Write(DataAdapter writer)
    {
        await writer.WriteFloat(Value);
    }
}
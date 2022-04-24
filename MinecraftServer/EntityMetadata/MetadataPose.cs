using MinecraftServer.Networking;

namespace MinecraftServer.EntityMetadata;

public class MetadataPose : IMetadataValue
{

    public int Type => 0x12;

    public int Pose { get; }

    public MetadataPose(int pose)
    {
        Pose = pose;
    }

    public async ValueTask Write(DataAdapter writer)
    {
        await writer.WriteVarInt(Pose);
    }
}
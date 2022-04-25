using MinecraftServer.Networking;

namespace MinecraftServer.EntityMetadata;

public class MetadataPose : IMetadataValue
{

    public int Type => 0x12;

    public Player.EntityPose Pose { get; }

    public MetadataPose(Player.EntityPose pose)
    {
        Pose = pose;
    }

    public async ValueTask Write(DataAdapter writer)
    {
        await writer.WriteVarIntAsync((int) Pose);
    }
}
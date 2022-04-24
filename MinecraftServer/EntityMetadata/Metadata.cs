using MinecraftServer.Networking;

namespace MinecraftServer.EntityMetadata;

public class Metadata
{

    public static async ValueTask WriteUpdate(DataAdapter writer, params Tuple<byte, IMetadataValue>[] updates)
    {
        foreach (var update in updates)
        {
            var (index, value) = update;

            await writer.WriteUByte(index);
            await writer.WriteVarInt(value.Type);
            await value.Write(writer);
        }
        await writer.WriteUByte(0xff);
    }
    
}

public interface IMetadataValue
{
    public int Type { get; }

    public ValueTask Write(DataAdapter writer);
}
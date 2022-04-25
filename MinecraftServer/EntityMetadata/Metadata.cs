using MinecraftServer.Networking;

namespace MinecraftServer.EntityMetadata;

public class Metadata
{

    public static async ValueTask WriteUpdate(DataAdapter writer, params Tuple<byte, IMetadataValue>[] updates)
    {
        foreach (var update in updates)
        {
            var (index, value) = update;

            await writer.WriteByteAsync(index);
            await writer.WriteVarIntAsync(value.Type);
            await value.Write(writer);
        }
        await writer.WriteByteAsync(0xff);
    }
    
}

public interface IMetadataValue
{
    public int Type { get; }

    public ValueTask Write(DataAdapter writer);
}
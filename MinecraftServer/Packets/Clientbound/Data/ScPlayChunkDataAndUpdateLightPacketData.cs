using MinecraftServer.Data;

namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayChunkDataAndUpdateLightPacketData : PacketData
{
    public int ChunkX { get; }
    public int ChunkZ { get; }
    public Chunk Chunk { get; }

    public ScPlayChunkDataAndUpdateLightPacketData(int chunkX, int chunkZ, Chunk chunk)
    {
        ChunkX = chunkX;
        ChunkZ = chunkZ;
        Chunk = chunk;
    }
}
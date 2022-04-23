namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayChunkDataAndUpdateLightPacketData : PacketData
{
    public int ChunkX { get; }
    public int ChunkZ { get; }

    public ScPlayChunkDataAndUpdateLightPacketData(int chunkX, int chunkZ)
    {
        ChunkX = chunkX;
        ChunkZ = chunkZ;
    }
}
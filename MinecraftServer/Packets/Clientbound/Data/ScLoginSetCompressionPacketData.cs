namespace MinecraftServer.Packets.Clientbound.Data;

public class ScLoginSetCompressionPacketData : PacketData
{
    public int CompressionThreshold { get; }

    public ScLoginSetCompressionPacketData(int size)
    {
        CompressionThreshold = size;
    }

    public override string ToString()
    {
        return $"ScLoginSetCompressionPacketData[CompressionThreshold={CompressionThreshold}]";
    }
    
}
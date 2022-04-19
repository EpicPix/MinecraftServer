namespace MinecraftServer.Packets.Clientbound.Data;

public class ScLoginSetCompressionPacketData : PacketData
{
    public int MaxPacketSize { get; }

    public ScLoginSetCompressionPacketData(int size)
    {
        MaxPacketSize = size;
    }
}
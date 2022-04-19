namespace MinecraftServer.Packets;

public struct UncompletedPacket
{
    public uint Offset;
    public byte[] Data;
}
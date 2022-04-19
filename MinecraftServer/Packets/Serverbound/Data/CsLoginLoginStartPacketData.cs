namespace MinecraftServer.Packets.Serverbound.Data;

public class CsLoginLoginStartPacketData : PacketData
{
    public string Name { get; }

    public CsLoginLoginStartPacketData(string name)
    {
        Name = name;
    }
}
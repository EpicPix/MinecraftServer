namespace MinecraftServer.Packets.Serverbound.Data;

public class CsLoginLoginStartPacketData : PacketData
{
    public string Name { get; }
    public Guid? ProfileId { get; }

    public CsLoginLoginStartPacketData(string name, Guid? profileId)
    {
        Name = name;
        ProfileId = profileId;
    }
}
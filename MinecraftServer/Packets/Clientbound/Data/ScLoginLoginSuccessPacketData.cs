namespace MinecraftServer.Packets.Clientbound.Data;

public class ScLoginLoginSuccessPacketData : PacketData
{
    public Guid Uuid { get; }
    public string Username { get; }
    
    public ScLoginLoginSuccessPacketData(Guid uuid, string username)
    {
        Uuid = uuid;
        Username = username;
    }

    public override string ToString()
    {
        return $"ScLoginLoginSuccessPacketData[Uuid={Uuid},Username={Username}]";
    }
    
}
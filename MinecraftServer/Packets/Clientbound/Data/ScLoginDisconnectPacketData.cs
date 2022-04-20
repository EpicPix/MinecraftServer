namespace MinecraftServer.Packets.Clientbound.Data;

public class ScLoginDisconnectPacketData : PacketData
{
    public ChatComponent Reason { get; }

    public ScLoginDisconnectPacketData(ChatComponent reason)
    {
        Reason = reason;
    }

    public override string ToString()
    {
        return $"ScLoginDisconnectPacketData[Reason={Reason}]";
    }

}
namespace MinecraftServer.Packets.Clientbound.Data;

public class ScDisconnectPacketData : PacketData
{
    public ChatComponent Reason { get; }

    public ScDisconnectPacketData(ChatComponent reason)
    {
        Reason = reason;
    }

    public override string ToString()
    {
        return $"ScDisconnectPacketData[Reason={Reason}]";
    }

}
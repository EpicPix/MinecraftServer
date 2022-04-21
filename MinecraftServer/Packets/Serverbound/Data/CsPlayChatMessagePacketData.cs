namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayChatMessagePacketData : PacketData
{

    public string Message { get; }

    public CsPlayChatMessagePacketData(string message)
    {
        Message = message;
    }

    public override string ToString()
    {
        return $"CsPlayChatMessagePacketData[Message={Message}]";
    }

}
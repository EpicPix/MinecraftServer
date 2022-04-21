namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayChatMessagePacketData : PacketData
{

    public enum PositionType
    {
        Chat = 0,
        SystemMessage = 1,
        GameInfo = 2
    }
    
    public ChatComponent Data { get; }
    public PositionType Position { get; }
    public Guid Sender { get; }

    public ScPlayChatMessagePacketData(ChatComponent data, PositionType position, Guid sender)
    {
        Data = data;
        Position = position;
        Sender = sender;
    }

    public override string ToString()
    {
        return $"ScPlayChatMessagePacketData[Data={Data},Position={Position},Sender={Sender}]";
    }

}
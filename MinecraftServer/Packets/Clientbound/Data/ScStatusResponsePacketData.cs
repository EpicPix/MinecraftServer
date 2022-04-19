namespace MinecraftServer.Packets.Clientbound.Data;

public class ScStatusResponsePacketData : PacketData
{
    public ServerInfo ServerInfo { get; }

    public ScStatusResponsePacketData(ServerInfo serverInfo)
    {
        ServerInfo = serverInfo;
    }
}
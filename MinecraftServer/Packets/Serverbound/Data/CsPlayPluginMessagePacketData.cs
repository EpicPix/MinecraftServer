using System.Text;

namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayPluginMessagePacketData : PacketData
{
    
    public string Channel { get; }
    public byte[] Data { get; }

    public CsPlayPluginMessagePacketData(string channel, byte[] data)
    {
        Channel = channel;
        Data = data;
    }

    public override string ToString()
    {
        return $"CsPlayPluginMessagePacketData[Channel={Channel},Data={Encoding.UTF8.GetString(Data)}]";
    }

}
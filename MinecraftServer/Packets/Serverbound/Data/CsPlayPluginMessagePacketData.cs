using System.Text;
using MinecraftServer.Networking;

namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayPluginMessagePacketData : PacketData, IDisposable
{
    
    public string Channel { get; }
    public IoBuffer Data { get; }

    public CsPlayPluginMessagePacketData(string channel, IoBuffer data)
    {
        Channel = channel;
        Data = data;
    }

    public override string ToString()
    {
        return $"CsPlayPluginMessagePacketData[Channel={Channel},Data={Encoding.UTF8.GetString(Data.Data)}]";
    }

    public void Dispose()
    {
        Data.Dispose();
    }
}
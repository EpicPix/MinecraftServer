using System.Text;
using MinecraftServer.Networking;

namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayPluginMessagePacketData : PacketData, IDisposable
{
    
    public string Channel { get; }
    public PooledArray Data { get; }

    public CsPlayPluginMessagePacketData(string channel, PooledArray data)
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
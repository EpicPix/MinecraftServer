using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Play;

namespace MinecraftServer.Events;

public static class TestPacketListener
{

    [PacketEvent(typeof(CsPlayChatMessage))]
    public static void Test(CsPlayChatMessagePacketData data, NetworkConnection connection, Server server)
    {
        Console.WriteLine($"Chat message packet event received {data.Message}");
    }
    
}
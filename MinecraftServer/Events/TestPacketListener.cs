using MinecraftServer.Networking;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Play;

namespace MinecraftServer.Events;

public static class TestPacketListener
{

    [PacketEvent(typeof(CsPlayChatMessage))]
    public static void Test(CsPlayChatMessagePacketData data, NetworkConnection connection, Server server)
    {
        Console.WriteLine($"Chat message (priority=0) packet event received {data.Message}");
    }
    
    [PacketEvent(typeof(CsPlayChatMessage), 50)]
    public static void Test2(CsPlayChatMessagePacketData data, NetworkConnection connection, Server server)
    {
        Console.WriteLine($"Chat message (priority=50) packet event received {data.Message}");
    }
    
    [PacketEvent(typeof(CsPlayChatMessage), -50)]
    public static void Test3(CsPlayChatMessagePacketData data, NetworkConnection connection, Server server)
    {
        Console.WriteLine($"Chat message (priority=-50) packet event received {data.Message}");
    }
    
}
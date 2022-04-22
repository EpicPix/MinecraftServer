using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Play;

namespace MinecraftServer.Events;

public static class TestPacketListener
{

    [PacketEvent(typeof(CsPlayChatMessage))]
    public static PacketEventHandlerStatus Test(CsPlayChatMessagePacketData data, NetworkConnection connection, Server server)
    {
        Console.WriteLine($"Chat message (priority=0) packet event received {data.Message}");
        return PacketEventHandlerStatus.Continue;
    }
    
    [PacketEvent(typeof(CsPlayChatMessage), 50)]
    public static PacketEventHandlerStatus Test2(CsPlayChatMessagePacketData data, NetworkConnection connection, Server server)
    {
        Console.WriteLine($"Chat message (priority=50) packet event received {data.Message}");
        return PacketEventHandlerStatus.Continue;
    }
    
    [PacketEvent(typeof(CsPlayChatMessage), -50)]
    public static PacketEventHandlerStatus Test3(CsPlayChatMessagePacketData data, NetworkConnection connection, Server server)
    {
        Console.WriteLine($"Chat message (priority=-50) packet event received {data.Message}");
        return PacketEventHandlerStatus.Continue;
    }
    
    [PacketEvent(typeof(ScPlayChatMessage))]
    public static PacketEventHandlerStatus Test4(ScPlayChatMessagePacketData data, NetworkConnection connection, Server server)
    {
        Console.WriteLine($"Outgoing chat message {data.Data}");
        return PacketEventHandlerStatus.Continue;
    }
    
}
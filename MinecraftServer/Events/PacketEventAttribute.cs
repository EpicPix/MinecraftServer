using MinecraftServer.Packets;

namespace MinecraftServer.Events;

[AttributeUsage(AttributeTargets.Method)]
public class PacketEventAttribute : Attribute
{
    public Packet Packet { get; }

    public PacketEventAttribute(Type packet)
    {
        Packet = Packet.GetPacket(packet);
    }
}
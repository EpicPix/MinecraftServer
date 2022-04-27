using MinecraftServer.Packets;

namespace MinecraftServer.Events;

// [AttributeUsage(AttributeTargets.Method)]
// public class PacketEventAttribute : Attribute
// {
//     public Packet Packet { get; }
//     public long Priority { get; }
//
//     public PacketEventAttribute(Type packet, long priority = 0)
//     {
//         Packet = Packet.GetPacket(packet);
//         Priority = priority;
//     }
// }
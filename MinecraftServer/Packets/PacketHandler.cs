using MinecraftServer.Packets.Clientbound;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Serverbound;
using MinecraftServer.Packets.Serverbound.Data;

namespace MinecraftServer.Packets;

public static class PacketHandler
{
    public static void HandlePacket(Server server, NetworkConnection connection, Packet packet)
    {
        var data = packet.ReadPacket(connection);

        if (packet is CsHandshake && data is CsHandshakePacketData handshake)
        {
            Console.WriteLine($"Protocol version: {handshake.ProtocolVersion} / Server IP: {handshake.ServerIp} / Server Port: {handshake.ServerPort} / Next State: {handshake.NextState}");
            if (handshake.NextState == (int) PacketType.Status)
            {
                connection.CurrentState = PacketType.Status;
            }else if (handshake.NextState == (int) PacketType.Login)
            {
                connection.CurrentState = PacketType.Login;
            } else
            {
                throw new NotSupportedException($"Unsupported packet type {handshake.NextState}");
            }
        }else if (packet is CsStatusRequest) {
            ScStatusResponse.Send(new (server.ServerInfo), connection);
        }else if (packet is CsStatusPing && data is CsStatusPingPacketData pingData) {
            ScStatusPong.Send(pingData, connection);
            connection.Connected = false;
        } else
        {
            throw new NotImplementedException($"Unsupported packet handler for packet {packet}");
        }
    }
    
}
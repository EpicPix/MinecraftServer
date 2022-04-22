using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Login;
using MinecraftServer.Packets.Clientbound.Status;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Handshake;
using MinecraftServer.Packets.Serverbound.Login;
using MinecraftServer.Packets.Serverbound.Play;
using MinecraftServer.Packets.Serverbound.Status;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{
    public static async Task HandlePacket(Server server, NetworkConnection connection, Packet packet, PacketData data)
    {
        foreach (var packetHandler in Server.PacketHandlers)
        {
            if (packetHandler.Packet == packet)
            {
                packetHandler.Run(data, connection, server);
            }
        }
        
        if (packet is CsHandshake && data is CsHandshakePacketData handshake)
        {
            Console.WriteLine($"Protocol version: {handshake.ProtocolVersion} / Server IP: {handshake.ServerIp} / Server Port: {handshake.ServerPort} / Next State: {(PacketType) handshake.NextState}");
            if (handshake.NextState == (int) PacketType.Status)
            {
                connection.ChangeState(PacketType.Status);
            }else if (handshake.NextState == (int) PacketType.Login)
            {
                connection.ChangeState(PacketType.Login);
            } else
            {
                throw new NotSupportedException($"Unsupported packet type {handshake.NextState}");
            }
        }
        else if (packet is CsStatusRequest)
        {
            await ScStatusResponse.Send(new (server.ServerInfo), connection);
        }
        else if (packet is CsStatusPing && data is CsStatusPingPacketData pingData)
        {
            await ScStatusPong.Send(pingData, connection);
            connection.Connected = false;
        }
        else if (packet is CsLoginLoginStart && data is CsLoginLoginStartPacketData loginData)
        {
            await HandleLoginStart(server, connection, loginData);
        }
        else if (packet is CsLoginEncryptionResponse && data is CsLoginEncryptionResponsePacketData loginEncryptionResponseData)
        {
            await HandleEncryptionResponse(server, connection, loginEncryptionResponseData);
        }
        else if (packet is CsPlayPluginMessage && data is CsPlayPluginMessagePacketData pluginMessageData)
        {
            Console.WriteLine($"Received plugin message: {pluginMessageData}");
        }
        else if (packet is CsPlayKeepAlive && data is ScPlayKeepAlivePacketData keepAliveData)
        {
            if (keepAliveData.KeepAliveId == connection.LastKeepAliveValue)
            {
                connection.LastKeepAlive = DateTime.UtcNow;
            }
        }
        else if (packet is CsPlayChatMessage && data is CsPlayChatMessagePacketData chatMessageData)
        {
            await HandlePacketChatMessage(server, connection, chatMessageData);
        }
    }
    
}
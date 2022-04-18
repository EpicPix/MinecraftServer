using System.Net;
using System.Net.Sockets;
using MinecraftServer;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Clientbound;
using MinecraftServer.Packets.Serverbound;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Clientbound.Data;


var serverInfo = new ServerInfo() with {
    description = new ServerInfo.DescriptionInfo() with 
    { 
        text = DateTime.UtcNow.ToString()
    }
};

var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Any, 25565));
server.Listen();



while (true)
{
    var client = await server.AcceptAsync();
    NetworkConnection connection = new NetworkConnection(client);
    await connection.ReadVarInt(); // packet length

    var packet = (CsHandshake) Packet.GetPacket(PacketType.Handshake, PacketSide.Client, (uint) await connection.ReadVarInt());
    var handshake = await packet.ReadPacket(connection);

    Console.WriteLine($"Protocol version: {handshake.ProtocolVersion} / Server IP: {handshake.ServerIp} / Server Port: {handshake.ServerPort} / Next State: {handshake.NextState}");

    if (handshake.NextState == 1)
    {
        await connection.ReadVarInt(); // packet length
        
        var handshakePackets = Packet.GetPacket(PacketType.Status, PacketSide.Client, (uint) await connection.ReadVarInt());
        
        if (handshakePackets is CsStatusRequest reqm) {
            await reqm.ReadPacket(connection);
            await ScStatusResponse.SendPacket(new ScStatusResponsePacketData(serverInfo), connection);
        }

        await connection.ReadVarInt();
        
        handshakePackets = Packet.GetPacket(PacketType.Status, PacketSide.Client, (uint) await connection.ReadVarInt());

        if (handshakePackets is CsStatusPing ping)
        {
            var pingData = await ping.ReadPacket(connection);
            await CsStatusPong.SendPacket(pingData, connection);
        }
    }
    
    // client.Close();
}
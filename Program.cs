using System.Net;
using System.Net.Sockets;
using MinecraftServer;
using MinecraftServer.Packets;



var serverInfo = new ServerInfo();

var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Any, 25565));
server.Listen();

static async Task SendPacket<T,R>(NetworkConnection connection, R data) where T : Packet<R> where R : PacketData
{
    var stream = new MemoryStream();
    var packet = Packet.GetPacket<T>();
    await packet.WritePacket(new NetworkConnection(stream), data);
    await connection.WriteVarInt((int) stream.Length + 1);
    await connection.WriteVarInt((int) packet.Id);
    await connection.WriteBytes(stream.GetBuffer());
    await connection.Flush();
}

while (true)
{
    var client = await server.AcceptAsync();
    NetworkConnection connection = new NetworkConnection(client);
    await connection.ReadVarInt(); // packet length

    var packet = (HandshakeClient) Packet.GetPacket(PacketType.Handshake, PacketSide.Client, (uint) await connection.ReadVarInt());
    var handshake = await packet.ReadPacket(connection);

    Console.WriteLine($"Protocol version: {handshake.ProtocolVersion} / Server IP: {handshake.ServerIp} / Server Port: {handshake.ServerPort} / Next State: {handshake.NextState}");

    if (handshake.NextState == 1)
    {
        await connection.ReadVarInt(); // packet length
        
        var handshakePackets = Packet.GetPacket(PacketType.Status, PacketSide.Client, (uint) await connection.ReadVarInt());
        
        if (handshakePackets is Packet<PacketData> req) 
            await req.ReadPacket(connection);

        if (handshakePackets is StatusRequestClient)
        {
            await SendPacket<StatusResponseServer, StatusResponsePacketData>(connection, new(serverInfo));
        }
        
        await connection.ReadVarInt();
        
        handshakePackets = Packet.GetPacket(PacketType.Status, PacketSide.Client, (uint) await connection.ReadVarInt());

        if (handshakePackets is StatusPingClient ping)
        {
            var pingData = await ping.ReadPacket(connection);
            await SendPacket<StatusPongClient, StatusPingPacketData>(connection, pingData);
        }
    }
    
    // client.Close();
}
using System.Net;
using System.Net.Sockets;
using MinecraftServer;
using MinecraftServer.Packets;

var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Any, 25565));
server.Listen();

var mcServer = new Server();

while (true)
{
    var client = await server.AcceptAsync();
    NetworkConnection connection = new NetworkConnection(client);

    while (connection.Connected)
    {
        await connection.ReadVarInt(); // packet length
        var packet = Packet.GetPacket(connection.CurrentState, PacketSide.Client, (uint) await connection.ReadVarInt());
        await PacketHandler.HandlePacket(mcServer, connection, packet);
    }
    
    // client.Close();
}
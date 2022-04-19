using System.Net;
using System.Net.Sockets;
using MinecraftServer;

var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Any, 25565));
server.Listen();

var mcServer = new Server();

Thread.CurrentThread.Name = "Socket Listener Thread";

while (true)
{
    var client = await server.AcceptAsync();
    NetworkConnection connection = new NetworkConnection(client);

    mcServer.Connections.Add(connection);
}
using System.Net;
using System.Net.Sockets;
using MinecraftServer;
using MinecraftServer.Networking;

Thread.CurrentThread.Name = "Socket Listener Thread";

var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Any, 25565));
server.Listen();
server.ReceiveTimeout = -1;
server.SendTimeout = -1;

var mcServer = new Server();

while (true)
{
    var client = await server.AcceptAsync();
    mcServer.AddConnection(new NetworkConnection(mcServer, new NetworkStream(client)));
}
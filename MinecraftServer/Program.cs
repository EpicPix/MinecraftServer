using System.Net;
using System.Net.Sockets;
using MinecraftServer;

Thread.CurrentThread.Name = "Socket Listener Thread";

var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Any, 25565));
server.Listen();
server.ReceiveTimeout = -1;
server.SendTimeout = -1;

var mcServer = new Server();

while (true)
{
    var client =  server.Accept();
    client.Blocking = false;
    mcServer.Connections.Add(new NetworkConnection(client, null, null));
}
using System.Net;
using System.Net.Sockets;
using MinecraftServer;
using MinecraftServer.Networking;
using MinecraftServer.SourceGeneration.Events;

public class Program
{
    public static async Task Main()
    {
        await DataBus.PostEventAsync("Hello, World!", new DataBus());
        return;
        Thread.CurrentThread.Name = "Socket Listener Thread";

        var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(new IPEndPoint(IPAddress.Any, 25565));
        server.Listen();
        server.SendTimeout = -1;
        server.ReceiveTimeout = -1;


        var mcServer = new Server();

        while (true)
        {
            var client = await server.AcceptAsync();
            mcServer.AddConnection(new NetworkConnection(mcServer, new NetworkStream(client)));
        }
    }
    
    [EventHandler(1, typeof(string), 100)]
    public static ValueTask TestingMethod(string val, DataBus bus)
    {
        Console.WriteLine("1-" + val);
        return ValueTask.CompletedTask;
    }
    [EventHandler(1, typeof(string), 101)]
    public static ValueTask TestingMethod1(string val, DataBus bus)
    {
        Console.WriteLine("2-" + val);
        return ValueTask.CompletedTask;
    }
    [EventHandler(1, typeof(string), 102)]
    public static ValueTask TestingMethod2(string val, DataBus bus)
    {
        Console.WriteLine("3-" + val);
        return ValueTask.CompletedTask;
    }
    [EventHandler(1, typeof(string), 103)]
    public static ValueTask TestingMethod3(string val, DataBus bus)
    {
        Console.WriteLine("4-" + val);
        return ValueTask.CompletedTask;
    }

}
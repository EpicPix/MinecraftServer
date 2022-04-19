using System.Collections.Concurrent;
using MinecraftServer.Packets;

namespace MinecraftServer;

public class Server
{
    public ServerInfo ServerInfo { get; }
    public Thread ServerThread { get; }
    public List<NetworkConnection> Connections { get; }
    public List<NetworkConnection> DeadConnections { get; }

    public Server()
    {
        ServerInfo = new ServerInfo {
            description = new ServerInfo.DescriptionInfo {
                text = DateTime.UtcNow.ToString()
            }
        };

        Connections = new();
        DeadConnections = new();
        ServerThread = new Thread(run);
        ServerThread.Name = "Server Thread";
        ServerThread.Start();
    }

    public void run()
    {
        while (true)
        {
            for (int i = 0; i < Connections.Count; i++) // more thread safe
            {
                UpdateConnection(Connections[i]);
            }

            DeadConnections.ForEach(connection => Connections.Remove(connection));
            DeadConnections.Clear();
        }
    }

    private void UpdateConnection(NetworkConnection connection)
    {
        if (!connection.Connected)
        {
            DeadConnections.Add(connection);
            Task.Delay(100).ContinueWith(_ => connection.Close());
            return;
        }

        if (!connection.CanRead)
        {
            return;
        }

        connection.ReadVarInt(); // packet length
        var packet = Packet.GetPacket(connection.CurrentState, PacketSide.Client, (uint) connection.ReadVarInt());
        PacketHandler.HandlePacket(this, connection, packet);

    }
}
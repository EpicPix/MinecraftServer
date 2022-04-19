using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Handlers;

namespace MinecraftServer;

public class Server
{
    public ServerInfo ServerInfo { get; }
    public Thread ServerThread { get; }
    public volatile List<NetworkConnection> Connections;
    public List<NetworkConnection> DeadConnections;
    public bool OnlineMode { get; } = false;
    internal RSA? RsaServer { get; }
    internal byte[]? ServerPublicKey { get; }

    public Server(bool isOnline = true)
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
        OnlineMode = isOnline;
        if (OnlineMode)
        {
            RsaServer = RSA.Create(1024);
            ServerPublicKey = RsaServer.ExportSubjectPublicKeyInfo();
        }
    }

    public void run()
    {
        while (true)
        {
            Thread.Sleep(1);
            for (int i = 0; i < Connections.Count; i++) // more thread safe
            {
                if(Connections[i] != null) UpdateConnection(Connections[i]);
            }

            foreach (var connection in DeadConnections)
            {
                Connections.Remove(connection);
            }
            DeadConnections.Clear();
        }
    }

    private void UpdateConnection(NetworkConnection connection)
    {
        if (!connection.Connected)
        {
            DeadConnections.Add(connection);
            return;
        }

        if (connection.CurrentPacket == null)
        {
            var read = (uint) connection.ReadVarIntBytes(out var length);
            if (read != 0)
            {
                connection.CurrentPacket = new UncompletedPacket {
                    Offset = 0,
                    Data = new byte[length]
                };
            }
        }

        if (connection.CurrentPacket != null)
        {
            if (connection.CurrentPacket.Value.Offset != connection.CurrentPacket.Value.Data.Length)
            {
                connection.ReadPacket();
            }

            if (connection.CurrentPacket.Value.Offset == connection.CurrentPacket.Value.Data.Length)
            {
                var currentReader = connection.Reader;

                using var memory = new MemoryStream(connection.CurrentPacket.Value.Data.ToArray());
                using var binaryReader = new BinaryReader(memory);
                connection.Reader = binaryReader;
                
                var packet = Packet.GetPacket(connection.CurrentState, PacketBound.Server, (uint) connection.ReadVarInt());
                PacketHandler.HandlePacket(this, connection, packet);

                
                connection.Reader = currentReader;
                connection.CurrentPacket = null;
            }
        }
    }
}
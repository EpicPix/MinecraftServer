using System.Buffers;
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Handlers;

namespace MinecraftServer;

public class Server
{
    public ServerInfo ServerInfo { get; }
    public volatile List<NetworkConnection> ActiveConnections;
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

        ActiveConnections = new();
        OnlineMode = isOnline;
        if (OnlineMode)
        {
            RsaServer = RSA.Create(1024);
            ServerPublicKey = RsaServer.ExportSubjectPublicKeyInfo();
        }
    }
    public void AddConnection(NetworkConnection connection)
    {
        Task.Run(() => HandleConnection(connection));
    }

    private async Task HandleConnection(NetworkConnection conn)
    {
        ActiveConnections.Add(conn);
        while (conn.Connected)
        {
            if (conn.IsCompressed)
            {
                // ignored for now
            }
            else
            {
                var length = await conn.ReadVarInt();
                var id = await conn.ReadVarInt();
                var packet = Packet.GetPacket(conn.CurrentState, PacketBound.Server, (uint) id);
                await PacketHandler.HandlePacket(this, conn, packet);
            }
        }

        ActiveConnections.Remove(conn);
    }
}
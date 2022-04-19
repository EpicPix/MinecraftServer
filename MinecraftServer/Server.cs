using System.Buffers;
using System.Formats.Asn1;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IO;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Handlers;

namespace MinecraftServer;

public class Server
{
    public static readonly RecyclableMemoryStreamManager MS_MANAGER = new RecyclableMemoryStreamManager();
    public ServerInfo ServerInfo { get; }
    public volatile List<NetworkConnection> ActiveConnections;
    public bool OnlineMode { get; } = false;
    internal RSA? RsaServer { get; }
    internal byte[]? ServerPublicKey { get; }
    public const int MaxPacketSize = 2097151;

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
                // var fullLength = await conn.ReadVarInt();
                // var dataLength = await conn.ReadVarInt();
                // var packet = PooledArray.Allocate(fullLength);
                // await conn.ReadBytes(packet);
                // using var ms = new MemoryStream(packet.Data.Array);
                // using var deflate = new DeflateStream(ms, CompressionMode.Decompress);
                // var decompPacket = PooledArray.Allocate(dataLength);
                // await Utils.FillBytes(decompPacket.Data, deflate);
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
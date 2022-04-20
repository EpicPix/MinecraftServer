using System.Buffers;
using System.Formats.Asn1;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IO;
using MinecraftServer.Networking;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Handlers;

namespace MinecraftServer;

public class Server
{
    public static readonly RecyclableMemoryStreamManager MS_MANAGER = new();
    public ServerInfo ServerInfo { get; }
    public volatile List<NetworkConnection> ActiveConnections;
    public bool OnlineMode { get; } = false;
    internal RSA? RsaServer { get; }
    internal byte[]? ServerPublicKey { get; }
    public const int NetworkCompressionThreshold = 256;

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
        Task.Run(() => HandleIncomingPackets(connection));
        Task.Run(() => connection.PacketQueue.ForwardPackets(connection));
    }

    private async Task HandleIncomingPackets(NetworkConnection conn)
    {
        ActiveConnections.Add(conn);
        try
        {
            while (conn.Connected)
            {
                if (conn.IsCompressed)
                {
                    var fullLength = await conn.ReadVarInt();
                    var dataLength = await conn.ReadVarInt();
                    if (dataLength != 0) conn.AddTransformer(x => new DecompressionAdapter(x));
                    try
                    {
                        var id = await conn.ReadVarInt();
                        var packet = Packet.GetPacket(conn.CurrentState, PacketBound.Server, (uint) id);
                        var data = await packet.ReadPacket(conn);

                        if (dataLength != 0) conn.PopTransformer();
                        await PacketHandler.HandlePacket(this, conn, packet, data);
                    } catch (ArgumentException e)
                    {
                        Console.WriteLine(e);
                        conn.Connected = false;
                    }
                    if (!conn.Connected) break;
                    if (dataLength != 0) conn.PopTransformer();
                }
                else
                {
                    var length = await conn.ReadVarInt();
                    var id = await conn.ReadVarInt();
                    var packet = Packet.GetPacket(conn.CurrentState, PacketBound.Server, (uint) id);
                    var data = await packet.ReadPacket(conn);
                    await PacketHandler.HandlePacket(this, conn, packet, data);
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            ActiveConnections.Remove(conn);
        }
    }
}
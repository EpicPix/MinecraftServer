using System.Net.Sockets;
using System.Security.Cryptography;
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
                    var packetDataLength = fullLength;

                    if (dataLength != 0)
                    {
                        conn.AddTransformer(x => new DecompressionAdapter(x));
                        packetDataLength = dataLength;
                    } else
                    {
                        packetDataLength -= Utils.GetVarIntLength(dataLength); // dataLength is 0, but i dont care
                    }
                    
                    var id = await conn.ReadVarInt();

                    packetDataLength -= Utils.GetVarIntLength(id);
                    if(!Packet.TryGetPacket(conn.CurrentState, PacketBound.Server, (uint)id, out var packet))
                    {
                        Console.WriteLine($"Unknown packet detected on state {conn.CurrentState} with id {id}. Skipping gracefully.");
                        await conn.Skip(packetDataLength);
                        if (dataLength != 0) conn.PopTransformer();
                        continue;
                    }
                    conn.PacketDataLength = (uint) packetDataLength;
                    var data = await packet.ReadPacket(conn);
                    
                    if (dataLength != 0) conn.PopTransformer();
                    
                    await PacketHandler.HandlePacket(this, conn, packet, data);
                    
                    if (!conn.Connected) break;
                }
                else
                {
                    var length = await conn.ReadVarInt();
                    var id = await conn.ReadVarInt();
                    var packetDataLength = length - Utils.GetVarIntLength(id);
                    if (!Packet.TryGetPacket(conn.CurrentState, PacketBound.Server, (uint)id, out var packet))
                    {
                        Console.WriteLine($"Unknown packet detected on state {conn.CurrentState} with id {id}. Skipping gracefully.");
                        await conn.Skip(packetDataLength);
                        continue;
                    }
                    conn.PacketDataLength = (uint) packetDataLength;
                    var data = await packet.ReadPacket(conn);
                    await PacketHandler.HandlePacket(this, conn, packet, data);
                }
            }
        }
        catch(Exception e)
        {
            if (e is not SocketException)
            {
                Console.WriteLine(e);
            }
        }
        finally
        {
            ActiveConnections.Remove(conn);
        }
    }
}
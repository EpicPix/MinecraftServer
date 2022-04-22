using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.IO;
using MinecraftServer.Events;
using MinecraftServer.Networking;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Handlers;

namespace MinecraftServer;

public class Server
{

    public static readonly IReadOnlyList<PacketEventHandler> PacketHandlers;

    internal static void GeneratePacketHandler<T>(MethodInfo method, Packet packet, List<PacketEventHandler> handlers) where T : PacketData
    {
        var delg = Delegate.CreateDelegate(typeof(Action<T, NetworkConnection, Server>), method);
        var handler = new PacketEventHandler<T>(packet, (Action<T, NetworkConnection, Server>) delg);
        handlers.Add(handler);
    }
    
    static Server()
    {
        var handlers = new List<PacketEventHandler>();
        
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach(var method in type.GetMethods())
            {
                var attr = (PacketEventAttribute?) method.GetCustomAttribute(typeof(PacketEventAttribute));
                if (attr != null)
                {
                    if (!method.IsStatic) throw new InvalidOperationException("Event Handler method must be static");

                    var ps = method.GetParameters();
                    if (ps.Length != 3) throw new InvalidOperationException("Event Handler method must have 3 parameters");

                    var packetDataType = attr.Packet.GetType().BaseType.GenericTypeArguments[1];
                    
                    if(ps[0].ParameterType != packetDataType) throw new InvalidOperationException($"{ps[0].ParameterType} != {packetDataType}");
                    if(ps[1].ParameterType != typeof(NetworkConnection)) throw new InvalidOperationException($"{ps[1].ParameterType} != {typeof(NetworkConnection)}");
                    if(ps[2].ParameterType != typeof(Server)) throw new InvalidOperationException($"{ps[2].ParameterType} != {typeof(Server)}");

                    var delegateGen = typeof(Server).GetMethod("GeneratePacketHandler", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(packetDataType);
                    
                    delegateGen.Invoke(null, BindingFlags.NonPublic | BindingFlags.Static, null, new object[]{ method, attr.Packet, handlers }, null);
                }
            }
        }

        PacketHandlers = handlers.AsReadOnly();
    }
    
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
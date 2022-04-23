using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.IO;
using MinecraftServer.Events;
using MinecraftServer.Networking;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Handlers;

namespace MinecraftServer;

public class Server
{

    public static readonly IReadOnlyList<PacketEventHandler> PacketHandlers;

    internal static void GeneratePacketHandler<T>(MethodInfo method, PacketEventAttribute attr, List<PacketEventHandler> handlers) where T : PacketData
    {
        if (method.GetParameters().Length == 3)
        {
            if (method.ReturnType == typeof(ValueTask))
            {
                var delg = Delegate.CreateDelegate(typeof(PacketEventHandler<T>.PacketEventHandlerFuncAsync), method);
                var handler = new PacketEventHandler<T>(attr.Packet, attr.Priority, (PacketEventHandler<T>.PacketEventHandlerFuncAsync) delg);
                handlers.Add(handler);
            } else
            {
                var delg = Delegate.CreateDelegate(typeof(PacketEventHandler<T>.PacketEventHandlerFunc), method);
                var handler = new PacketEventHandler<T>(attr.Packet, attr.Priority, (PacketEventHandler<T>.PacketEventHandlerFunc) delg);
                handlers.Add(handler);
            }
        } else
        {
            if (method.ReturnType == typeof(ValueTask))
            {
                var delg = Delegate.CreateDelegate(typeof(PacketEventHandler<T>.PacketEventHandlerFuncStatusAsync), method);
                var handler = new PacketEventHandler<T>(attr.Packet, attr.Priority, (PacketEventHandler<T>.PacketEventHandlerFuncStatusAsync) delg);
                handlers.Add(handler);
            } else
            {
                var delg = Delegate.CreateDelegate(typeof(PacketEventHandler<T>.PacketEventHandlerFuncStatus), method);
                var handler = new PacketEventHandler<T>(attr.Packet, attr.Priority, (PacketEventHandler<T>.PacketEventHandlerFuncStatus) delg);
                handlers.Add(handler);
            }
        }
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
                    if (ps.Length != 3 && ps.Length != 4) throw new InvalidOperationException("Event Handler method must have 3 or 4 parameters");

                    var packetDataType = attr.Packet.GetType().BaseType.GenericTypeArguments[1];
                    
                    if(ps[0].ParameterType != packetDataType) throw new InvalidOperationException($"{ps[0].ParameterType} != {packetDataType}");
                    if(ps[1].ParameterType != typeof(NetworkConnection)) throw new InvalidOperationException($"{ps[1].ParameterType} != {typeof(NetworkConnection)}");
                    if(ps[2].ParameterType != typeof(Server)) throw new InvalidOperationException($"{ps[2].ParameterType} != {typeof(Server)}");

                    if (ps.Length == 4)
                    {
                        if(ps[3].ParameterType != typeof(PacketEventHandlerStatus).MakeByRefType()) throw new InvalidOperationException($"{ps[2].ParameterType} != {typeof(PacketEventHandlerStatus).MakeByRefType()}");
                    }

                    var handlerAdder = typeof(Server).GetMethod("GeneratePacketHandler", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(packetDataType);
                    handlerAdder.Invoke(null, BindingFlags.NonPublic | BindingFlags.Static, null, new object[]{ method, attr, handlers }, null);
                }
            }
        }
        handlers.Sort((a, b) => -a.Priority.CompareTo(b.Priority));
        PacketHandlers = handlers.AsReadOnly();
    }
    
    public static readonly RecyclableMemoryStreamManager MS_MANAGER = new();
    public ServerInfo ServerInfo { get; }
    public volatile List<NetworkConnection> ActiveConnections;
    public volatile List<Player> Players;
    public bool OnlineMode { get; } = false;
    internal RSA? RsaServer { get; }
    internal byte[]? ServerPublicKey { get; }
    private uint _currentEntityId;
    public const int NetworkCompressionThreshold = 256;

    public Server(bool isOnline = true)
    {
        ServerInfo = new ServerInfo {
            description = new ServerInfo.DescriptionInfo {
                text = DateTime.UtcNow.ToString()
            }
        };

        ActiveConnections = new();
        Players = new();
        OnlineMode = isOnline;
        if (OnlineMode)
        {
            RsaServer = RSA.Create(1024);
            ServerPublicKey = RsaServer.ExportSubjectPublicKeyInfo();
        }
    }

    public uint NextEntityId()
    {
        return _currentEntityId++;
    }

    public void BroadcastMessage(ChatComponent message)
    {
        Console.WriteLine(message.text);
        var joinPacketData = new ScPlayChatMessagePacketData(message, ScPlayChatMessagePacketData.PositionType.Chat, Guid.Empty);
        foreach (var onlinePlayer in Players)
        {
            ScPlayChatMessage.Send(joinPacketData, onlinePlayer.Connection);
        }
    }

    public void OnPlayerJoin(Player player)
    {
        BroadcastMessage(new ChatComponent($"{player.Username} joined"));

        var actions = new List<ScPlayPlayerInfoPacketData.IAction>();
        foreach (var onlinePlayer in Players)
        {
            var action = new ScPlayPlayerInfoPacketData.AddPlayerAction {
                Uuid = onlinePlayer.Uuid,
                Username = onlinePlayer.Username,
                Profile = onlinePlayer.Connection.Profile,
                Gamemode = 1,
                Ping = 0,
                DisplayName = null
            };
            actions.Add(action);
            if (player == onlinePlayer)
            {
                foreach (var eonlinePlayer in Players)
                {
                    if (eonlinePlayer == player) continue;
                    ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.AddPlayer, new List<ScPlayPlayerInfoPacketData.IAction> { action }), eonlinePlayer.Connection);
                }
                continue;
            }
            ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.AddPlayer, new List<ScPlayPlayerInfoPacketData.IAction> { action }), onlinePlayer.Connection);
        }
        
        ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.AddPlayer, actions), player.Connection);
    }

    public void OnPlayerLeave(Player player)
    {
        BroadcastMessage(new ChatComponent($"{player.Username} left"));
        
        foreach (var onlinePlayer in Players)
        {
            ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.RemovePlayer, new List<ScPlayPlayerInfoPacketData.IAction> {
                new ScPlayPlayerInfoPacketData.RemovePlayerAction {
                    Uuid = player.Uuid
                }
            }), onlinePlayer.Connection);
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
            while (!conn.EndOfPhysicalStream)
            {
                Packet? packet;
                long curPos, expectedLength, readableUncompressedLength;
                bool requirePopTransform = false;
                if (conn.IsCompressed)
                {
                    expectedLength = await conn.ReadVarInt();
                    curPos = conn.RawBytesRead;
                    var dataLength = await conn.ReadVarInt();
                    var packetDataLength = expectedLength;

                    var decompAllowedLength = packetDataLength - Utils.GetVarIntLength(dataLength);
                    if (dataLength != 0)
                    {
                        conn.AddTransformer((x, ct) =>
                        {
                            return new DecompressionAdapter(new StreamAdapter(x, decompAllowedLength, ct), ct);
                        }, true);
                        requirePopTransform = true;
                        packetDataLength = dataLength;
                    } 
                    else
                    {
                        packetDataLength -= Utils.GetVarIntLength(dataLength); // dataLength is 0, but i dont care
                    }
                    
                    var id = await conn.ReadVarInt();

                    packetDataLength -= Utils.GetVarIntLength(id);
                    if(!Packet.TryGetPacket(conn.CurrentState, PacketBound.Server, (uint)id, out packet))
                    {
                        Console.WriteLine($"Unknown packet detected on state {conn.CurrentState} with id {id}. Skipping gracefully.");
                        goto ProcessCorruption;
                    }

                    readableUncompressedLength = packetDataLength;
                }
                else
                {
                    expectedLength = await conn.ReadVarInt();
                    curPos = conn.RawBytesRead;
                    var id = await conn.ReadVarInt();
                    var packetDataLength = expectedLength - Utils.GetVarIntLength(id);
                    if (!Packet.TryGetPacket(conn.CurrentState, PacketBound.Server, (uint)id, out packet))
                    {
                        Console.WriteLine($"Unknown packet detected on state {conn.CurrentState} with id {id}. Skipping gracefully.");
                        goto ProcessCorruption;
                    }
                    readableUncompressedLength = packetDataLength;
                }
                var data = await packet.ReadPacket(new StreamAdapter(conn, readableUncompressedLength, conn.ConnectionState));

                foreach (var packetHandler in PacketHandlers)
                {
                    if (packetHandler.Packet == packet)
                    {
                        var status = PacketEventHandlerStatus.Continue;
                        if (packetHandler.Async)
                        {
                            var reference = new PacketEventHandlerStatusRef(status);
                            await packetHandler.RunAsync(data, conn, this, reference);
                            status = reference.HandlerStatus;
                        } else
                        {
                            packetHandler.Run(data, conn, this, ref status);
                        }
                        if ((status & PacketEventHandlerStatus.Stop) == PacketEventHandlerStatus.Stop)
                        {
                            break;
                        }
                    }
                }
                
                ProcessCorruption:
                if(requirePopTransform) conn.PopTransformer(true);
                long readLength = conn.RawBytesRead - curPos;
                if (readLength != expectedLength)
                {
                    if (readLength < expectedLength)
                    {
                        Console.WriteLine($"Packet reading corruption detected for {packet}: Expected Length {expectedLength} bytes. Read {readLength} bytes. {expectedLength - readLength} bytes will be skipped.");
                        await conn.Skip((int)(expectedLength - readLength));
                    }
                    else
                    {
                        Console.WriteLine($"Packet reading corruption detected for {packet}: Expected Length {expectedLength} bytes. Read {readLength} bytes. This is not recoverable. The connection will be closed.");
                        await conn.Disconnect("", true);
                    }
                }
                
                if (!conn.Connected) break;
            }
        }
        catch(Exception e)
        {
            if (e is not SocketException)
            {
                Console.WriteLine(e);
            }

            if (conn.Connected)
            {
                await conn.Disconnect("", true);
            }
        }
        finally
        {
            conn.PacketQueue.Stop();
            ActiveConnections.Remove(conn);
            if (conn.Player != null)
            {
                Players.Remove(conn.Player);
                OnPlayerLeave(conn.Player);
            }
        }
    }
}
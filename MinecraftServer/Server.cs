using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.IO;
using MinecraftServer.Data;
using MinecraftServer.EntityMetadata;
using MinecraftServer.Events;
using MinecraftServer.Networking;
using MinecraftServer.Packets;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Play;

namespace MinecraftServer;

public class Server
{

    public static readonly RecyclableMemoryStreamManager MS_MANAGER = new();
    public ServerInfo ServerInfo { get; }
    public volatile List<NetworkConnection> ActiveConnections;
    public volatile List<Player> Players;
    public bool OnlineMode { get; } = false;
    internal RSA? RsaServer { get; }
    internal byte[]? ServerPublicKey { get; }
    private uint _currentEntityId;
    public string Brand { get; }
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
        Brand = "§k§l§o§nCustom... Text don't you love putting actual text where it shouldn't be§r";
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
//        foreach (var onlinePlayer in Players)
//        {
//            onlinePlayer.SendMessage(message);
//        }
    }

    public void OnPlayerJoin(Player player)
    {
        BroadcastMessage(new ChatComponent($"{player.Username} joined"));
        
        
        // TODO Move this to the first tick of the player joining
        var actions = new List<ScPlayPlayerInfoPacketData.IAction>();
        var actionsListed = new List<ScPlayPlayerInfoPacketData.IAction>();
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
            var actionListed = new ScPlayPlayerInfoPacketData.UpdateListedAction {
                Uuid = onlinePlayer.Uuid,
                Listed = true
            };
            actionsListed.Add(actionListed);
            if (player == onlinePlayer)
            {
                foreach (var eonlinePlayer in Players)
                {
                    if (eonlinePlayer == player) continue;
                    ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.AddPlayer, new List<ScPlayPlayerInfoPacketData.IAction> { action }), eonlinePlayer.Connection);
                    ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.UpdateListed, new List<ScPlayPlayerInfoPacketData.IAction> { actionListed }), eonlinePlayer.Connection);
                }
                continue;
            }
            ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.AddPlayer, new List<ScPlayPlayerInfoPacketData.IAction> { action }), onlinePlayer.Connection);
            ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.UpdateListed, new List<ScPlayPlayerInfoPacketData.IAction> { actionListed }), onlinePlayer.Connection);
        }
        ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.AddPlayer, actions), player.Connection);
        ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.UpdateListed, actionsListed), player.Connection);
        var spawn = new ScPlaySpawnPlayerPacketData((int) player.EntityId, player.Uuid, player.ClientX, player.ClientY, player.ClientZ, player.Yaw, player.Pitch);
        foreach (var eonlinePlayer in Players)
        {
            if (eonlinePlayer == player) continue;

            ScPlaySpawnPlayer.Send(
                new ScPlaySpawnPlayerPacketData((int) eonlinePlayer.EntityId, eonlinePlayer.Uuid, eonlinePlayer.ClientX, eonlinePlayer.ClientY, eonlinePlayer.ClientZ, eonlinePlayer.Yaw, eonlinePlayer.Pitch),
                player.Connection);
            if (eonlinePlayer.EntityFlags != 0)
            {
                ScPlayEntityMetadata.Send(new ScPlayEntityMetadataPacketData((int) eonlinePlayer.EntityId, new Tuple<byte, IMetadataValue>(0, new MetadataByte(eonlinePlayer.EntityFlags))), player.Connection);
            }
            ScPlaySpawnPlayer.Send(spawn, eonlinePlayer.Connection);
        }
    }

    public void OnPlayerLeave(Player player)
    {
        BroadcastMessage(new ChatComponent($"{player.Username} left"));
        
        foreach (var onlinePlayer in Players)
        {
            ScPlayPlayerInfoRemove.Send(new ScPlayPlayerInfoRemovePacketData(new List<Guid> { player.Uuid }), onlinePlayer.Connection);
            ScPlayDestroyEntities.Send(new ScPlayDestroyEntitiesPacketData(new List<int> { (int) player.EntityId }), onlinePlayer.Connection);
        }
    }

    private Dictionary<ulong, Chunk> _chunks = new();

    public Chunk GetChunk(int x, int z)
    {
        var v = (uint) x | ((ulong) z << 32);
        if (_chunks.ContainsKey(v))
        {
            return _chunks[v];
        }
        
        var c = new Chunk(256);
        var yLevel = 63;
        for (byte cx = 0; cx <= 15; cx++)
        {
            for (byte cz = 0; cz <= 15; cz++)
            {
                c[yLevel / 16][(cx, (byte) (yLevel % 16), cz)] = BlockState.GrassBlock;
            }
        }
        _chunks[v] = c;
        return c;
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
                    expectedLength = await conn.ReadVarIntAsync();
                    curPos = conn.RawBytesRead;
                    var dataLength = await conn.ReadVarIntAsync();
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
                    
                    var id = await conn.ReadVarIntAsync();

                    packetDataLength -= Utils.GetVarIntLength(id);
                    if(!Packet.TryGetPacket(conn.CurrentState, PacketBound.Server, (uint)id, out packet))
                    {
                        Console.WriteLine($"Unknown packet detected on state {conn.CurrentState} with id 0x{id:x}. Skipping gracefully.");
                        goto PacketCorruption;
                    }

                    readableUncompressedLength = packetDataLength;
                }
                else
                {
                    expectedLength = await conn.ReadVarIntAsync();
                    curPos = conn.RawBytesRead;
                    var id = await conn.ReadVarIntAsync();
                    var packetDataLength = expectedLength - Utils.GetVarIntLength(id);
                    if (!Packet.TryGetPacket(conn.CurrentState, PacketBound.Server, (uint)id, out packet))
                    {
                        Console.WriteLine($"Unknown packet detected on state {conn.CurrentState} with id 0x{id:x}. Skipping gracefully.");
                        goto PacketCorruption;
                    }
                    readableUncompressedLength = packetDataLength;
                }
                var data = await packet.ReadPacket(new StreamAdapter(conn, readableUncompressedLength, conn.ConnectionState));

                await PacketEventBus.PostEventAsync(data, conn.EventBus);

                conn.Player?.Tick(); // TODO Remove this when there is actual server ticking

                PacketCorruption:
                if(requirePopTransform) conn.PopTransformer(true);
                long readLength = conn.RawBytesRead - curPos;
                if (readLength != expectedLength)
                {
                    if (readLength < expectedLength)
                    {
                        Console.WriteLine($"Packet reading corruption detected for {packet}: Expected Length {expectedLength} bytes. Read {readLength} bytes. {expectedLength - readLength} bytes will be skipped.");
                        await conn.SkipAsync((int)(expectedLength - readLength));
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
            if (e is not SocketException && e is not IOException)
            {
                Console.WriteLine(e);
            }

            if (conn.Connected)
            {
                await conn.Disconnect("", true);
            }
        }
        conn.PacketQueue.Stop();
        ActiveConnections.Remove(conn);
        if (conn.Player != null)
        {
            Players.Remove(conn.Player);
            OnPlayerLeave(conn.Player);
        }
    }
}
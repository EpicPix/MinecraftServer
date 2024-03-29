﻿using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using MinecraftServer.Events;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Login;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Login;
using MinecraftServer.SourceGenerators.Events;
using MinecraftServer.Types;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{
    [EventHandler(EventBuses.Packet, typeof(CsLoginEncryptionResponsePacketData), priority: 100, true)]
    public static async ValueTask HandleEncryptionResponse(CsLoginEncryptionResponsePacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        Debug.Assert(server.OnlineMode);
        var decryptedToken = server.RsaServer!.Decrypt(data.VerifyToken, RSAEncryptionPadding.Pkcs1);
        Debug.Assert(decryptedToken.SequenceEqual(connection.VerifyToken));
        
        connection.EncryptionKey = server.RsaServer.Decrypt(data.SharedSecret, RSAEncryptionPadding.Pkcs1);
        await using var ms = new MemoryStream();
        await ms.WriteAsync(Encoding.ASCII.GetBytes(""));
        await ms.WriteAsync(connection.EncryptionKey);
        await ms.WriteAsync(server.ServerPublicKey);
        var hash = Utils.MinecraftShaDigest(ms.ToArray());

        using var client = new HttpClient();
        var uriBuilder = new UriBuilder("https://sessionserver.mojang.com/session/minecraft/hasJoined");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["username"] = connection.Username;
        query["serverId"] = hash;
        uriBuilder.Query = query.ToString();
        var result = await client.GetAsync(uriBuilder.ToString());

        if (result.StatusCode != HttpStatusCode.OK)
        {
            ScLoginDisconnect.Send(new ScDisconnectPacketData(new ChatComponent("Authentication Failed")), connection);
            throw new Exception("Not authenticated with Mojang");
        }
        
        connection.Profile = JsonSerializer.Deserialize(await result.Content.ReadAsStringAsync(), SerializationContext.Default.GameProfile!);
        connection.Username = connection.Profile!.name;
        connection.Uuid = connection.Profile.Uuid;
        Console.WriteLine($"Player has connected with UUID {connection.Uuid}, and username '{connection.Username}'");
        connection.Encrypt();
        Console.WriteLine(@"Stream is now encrypted");
        
        FinishLogin(connection, server);
    }

    [EventHandler(EventBuses.Packet, typeof(CsLoginLoginStartPacketData), priority: 100)]
    public static void HandleLoginStart(CsLoginLoginStartPacketData data, PacketEventBus bus)
    {
        var (server, connection) = (bus.Server, bus.Connection);
        connection.Username = data.Name;
        if (!server.OnlineMode)
        {
            var uuid = Utils.GuidFromString($"OfflinePlayer:{connection.Username}");
            connection.Uuid = uuid;
            
            FinishLogin(connection, server);
            return;
        }

        connection.VerifyToken = RandomNumberGenerator.GetBytes(4);
        var encryptionReq = new ScLoginEncryptionRequestPacketData("", server.ServerPublicKey!, connection.VerifyToken);
        ScLoginEncryptionRequest.Send(encryptionReq, connection);
    }

    private static void FinishLogin(NetworkConnection connection, Server server)
    {
        if (Server.NetworkCompressionThreshold > 0)
        {
            ScLoginSetCompression.Send(new ScLoginSetCompressionPacketData(Server.NetworkCompressionThreshold), connection, conn => {
                conn.IsCompressed = true;
                return ValueTask.CompletedTask;
            });
        }


        ScLoginLoginSuccess.Send(new ScLoginLoginSuccessPacketData(connection.Uuid, connection.Username), connection);
        
        connection.Player = new Player(server, connection, server.NextEntityId());
        server.Players.Add(connection.Player);
        
        connection.ChangeState(PacketType.Play);
        ScPlayJoinGame.Send(new ScPlayJoinGamePacketData((int) connection.Player.EntityId), connection);
        connection.Player.Teleport(0, 80, 0);
        connection.Player.Tick();
        var b = IoBuffer.Allocate(Encoding.UTF8.GetByteCount(server.Brand));
        Encoding.UTF8.GetBytes(server.Brand, b.Data);
        ScPlayPluginMessage.Send(new CsPlayPluginMessagePacketData("minecraft:brand", b), connection, _ => {
            b.Dispose();
            return ValueTask.CompletedTask;
        });
        ScPlaySpawnPosition.Send(new ScPlaySpawnPositionPacketData(new Position(0, 80, 0), 0), connection);
        server.OnPlayerJoin(connection.Player);
        for (var x = -4; x <= 4; x++)
        {
            for (var z = -4; z <= 4; z++)
            {
                if (x * x + z * z < 4 * 4)
                {
                    if (!connection.SentChunks.ContainsKey((x, z)))
                    {
                        connection.SentChunks[(x, z)] = true;
                        ScPlayChunkDataAndUpdateLight.Send(new ScPlayChunkDataAndUpdateLightPacketData(x, z, server.GetChunk(x, z)), connection);
                    }
                }
            }
        }
    }
    
}
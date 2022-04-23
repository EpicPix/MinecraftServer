using System.Diagnostics;
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

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{
    [PacketEvent(typeof(CsLoginEncryptionResponse), priority: 100)]
    public static async ValueTask HandleEncryptionResponse(CsLoginEncryptionResponsePacketData data, NetworkConnection connection, Server server)
    {
        Debug.Assert(server.OnlineMode);
        var decryptedToken = server.RsaServer.Decrypt(data.VerifyToken, RSAEncryptionPadding.Pkcs1);
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
            await ScLoginDisconnect.SendAsync(new ScDisconnectPacketData(new ChatComponent("Authentication Failed")), connection);
            throw new Exception("Not authenticated with Mojang");
        }
        
        connection.Profile = JsonSerializer.Deserialize(await result.Content.ReadAsStringAsync(), SerializationContext.Default.GameProfile!);
        connection.Username = connection.Profile.name;
        connection.Uuid = connection.Profile.Uuid;
        Console.WriteLine($"Player has connected with info: {JsonSerializer.Serialize(connection.Profile, SerializationContext.Default.GameProfile)}");
        connection.Encrypt();
        Console.WriteLine(@"Stream is now encrypted");
        
        FinishLogin(connection, server);
    }

    [PacketEvent(typeof(CsLoginLoginStart), priority: 100)]
    public static void HandleLoginStart(CsLoginLoginStartPacketData data, NetworkConnection connection, Server server)
    {
        connection.Username = data.Name;
        if (!server.OnlineMode)
        {
            var uuid = Utils.GuidFromString($"OfflinePlayer:{connection.Username}");
            connection.Uuid = uuid;
            
            FinishLogin(connection, server);
            return;
        }

        connection.VerifyToken = RandomNumberGenerator.GetBytes(4);
        var encryptionReq = new ScLoginEncryptionRequestPacketData("", server.ServerPublicKey, connection.VerifyToken);
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
        connection.ChangeState(PacketType.Play);
        ScPlayJoinGame.Send(new ScPlayJoinGamePacketData(), connection);
        ScPlayPlayerPositionAndLook.Send(new ScPlayPlayerPositionAndLookPacketData(0, 64, 0, 0, 0, 0x0, 0, false), connection);
        ScPlayChatMessage.Send(new ScPlayChatMessagePacketData(new ChatComponent("Test message, in online mode"), ScPlayChatMessagePacketData.PositionType.Chat, Guid.Empty), connection);
        ScPlayUpdateViewPosition.Send(new ScPlayUpdateViewPositionPacketData(0, 0), connection);
        ScPlayChunkDataAndUpdateLight.Send(null, connection);
        ScPlayPlayerInfo.Send(new ScPlayPlayerInfoPacketData(ScPlayPlayerInfoPacketData.UpdateAction.AddPlayer, new List<ScPlayPlayerInfoPacketData.IAction> {
            new ScPlayPlayerInfoPacketData.AddPlayerAction {
                Uuid = connection.Uuid,
                Username = connection.Username,
                Profile = connection.Profile,
                Gamemode = 1,
                Ping = 0,
                DisplayName = null
            }
        }), connection);
    }
    
}
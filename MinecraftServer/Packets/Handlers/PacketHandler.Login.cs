using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using MinecraftServer.Networking;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Login;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Login;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{
    public static async ValueTask HandleEncryptionResponse(Server server, NetworkConnection connection, CsLoginEncryptionResponsePacketData data)
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
            await ScLoginDisconnect.Send(new ScDisconnectPacketData(new ChatComponent("Authentication Failed")), connection);
            throw new Exception("Not authenticated with Mojang");
        }

        connection.Profile = JsonSerializer.Deserialize(await result.Content.ReadAsStringAsync(), SerializationContext.Default.GameProfile!);
        Console.WriteLine($"Player has connected with info: {JsonSerializer.Serialize(connection.Profile, SerializationContext.Default.GameProfile!)}");
        connection.Encrypt();
        Console.WriteLine(@"Stream is now encrypted");

        if (Server.NetworkCompressionThreshold > 0)
        {
            await ScLoginSetCompression.Send(new ScLoginSetCompressionPacketData(Server.NetworkCompressionThreshold), connection, conn => {
                conn.IsCompressed = true;
                return ValueTask.CompletedTask;
            });
        }


        await ScLoginLoginSuccess.Send(new ScLoginLoginSuccessPacketData(connection.Profile.Uuid, connection.Profile.name), connection);
        connection.ChangeState(PacketType.Play);
        await ScPlayJoinGame.Send(new ScPlayJoinGamePacketData(), connection);
        await ScPlayPlayerPositionAndLook.Send(new ScPlayPlayerPositionAndLookPacketData(0, 64, 0, 0, 0, 0x0, 0, false), connection);
        await ScPlayChatMessage.Send(new ScPlayChatMessagePacketData(new ChatComponent("Test message, in online mode"), ScPlayChatMessagePacketData.PositionType.Chat, Guid.Empty), connection);
    }

    public static async ValueTask HandleLoginStart(Server server, NetworkConnection connection, CsLoginLoginStartPacketData data)
    {
        connection.Username = data.Name;
        if (!server.OnlineMode)
        {
            if (Server.NetworkCompressionThreshold > 0)
            {
                await ScLoginSetCompression.Send(new ScLoginSetCompressionPacketData(Server.NetworkCompressionThreshold), connection, conn => {
                    conn.IsCompressed = true;
                    return ValueTask.CompletedTask;
                });
            }

            await ScLoginLoginSuccess.Send(new ScLoginLoginSuccessPacketData(Utils.GuidFromString($"OfflinePlayer:{connection.Username}"), connection.Username), connection);
            connection.ChangeState(PacketType.Play);
            await ScPlayJoinGame.Send(new ScPlayJoinGamePacketData(), connection);
            await ScPlayPlayerPositionAndLook.Send(new ScPlayPlayerPositionAndLookPacketData(0, 64, 0, 0, 0, 0x0, 0, false), connection);
            await ScPlayChatMessage.Send(new ScPlayChatMessagePacketData(new ChatComponent("Test message, in offline mode"), ScPlayChatMessagePacketData.PositionType.Chat, Guid.Empty), connection);
            // await ScLoginDisconnect.Send(new ScDisconnectPacketData(new ChatComponent($"{data.Name}")), connection);
            // connection.Connected = false;
            return;
        }

        connection.VerifyToken = RandomNumberGenerator.GetBytes(4);
        var encryptionReq = new ScLoginEncryptionRequestPacketData("", server.ServerPublicKey, connection.VerifyToken);
        await ScLoginEncryptionRequest.Send(encryptionReq, connection);
    }
}
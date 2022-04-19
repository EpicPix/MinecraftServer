using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using MinecraftServer.Packets.Clientbound.Data;
using MinecraftServer.Packets.Clientbound.Login;
using MinecraftServer.Packets.Clientbound.Play;
using MinecraftServer.Packets.Serverbound.Data;
using MinecraftServer.Packets.Serverbound.Login;

namespace MinecraftServer.Packets.Handlers;

public static partial class PacketHandler
{
    public static void HandleEncryptionResponse(Server server, NetworkConnection connection, CsLoginEncryptionResponsePacketData data)
    {
        Debug.Assert(server.OnlineMode);
        var decryptedToken = server.RsaServer.Decrypt(data.VerifyToken, RSAEncryptionPadding.Pkcs1);
        Debug.Assert(decryptedToken.SequenceEqual(connection.VerifyToken));
        
        connection.EncryptionKey = server.RsaServer.Decrypt(data.SharedSecret, RSAEncryptionPadding.Pkcs1);
        using var ms = new MemoryStream();
        ms.Write(Encoding.ASCII.GetBytes(""));
        ms.Write(connection.EncryptionKey);
        ms.Write(server.ServerPublicKey);
        var hash = Utils.MinecraftShaDigest(ms.ToArray());

        var client = new HttpClient();
        var uriBuilder = new UriBuilder("https://sessionserver.mojang.com/session/minecraft/hasJoined");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["username"] = connection.Username;
        query["serverId"] = hash;
        uriBuilder.Query = query.ToString();
        var result = client.GetAsync(uriBuilder.ToString()).Result;

        if ((int)result.StatusCode != 200)
        {
            throw new Exception("Not authenticated with Mojang");
        }
        Console.WriteLine(@"Player has connected with info: " + result.Content.ReadAsStringAsync().Result);
        
        
    }

    public static void HandleLoginStart(Server server, NetworkConnection connection, CsLoginLoginStartPacketData data)
    {
        connection.Username = data.Name;
        if (!server.OnlineMode)
        {
            ScLoginLoginSuccess.Send(new ScLoginLoginSuccessPacketData(Utils.GuidFromString($"OfflinePlayer:{connection.Username}"), connection.Username), connection);
            connection.CurrentState = PacketType.Play;

            ScPlayJoinGame.Send(new ScPlayJoinGamePacketData(), connection);
            // ScPlayPlayerPositionAndLook.Send(new ScPlayPlayerPositionAndLookPacketData(0, 64, 0, 0, 0, 0x0, 0, false), connection);
            // ScLoginDisconnect.Send(new ScLoginDisconnectPacketData(new ChatComponent($"{loginData.Name}")), connection);
            // connection.Connected = false;
            return;
        }

        connection.VerifyToken = RandomNumberGenerator.GetBytes(4);
        var encryptionReq = new ScLoginEncryptionRequestPacketData("", server.ServerPublicKey, connection.VerifyToken);
        ScLoginEncryptionRequest.Send(encryptionReq, connection);
    }
}
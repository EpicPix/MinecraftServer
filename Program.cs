using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using MinecraftServer;

var serverInfo = new ServerInfo();


var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Any, 25565));
server.Listen();

while (true)
{
    var client = await server.AcceptAsync();
    NetworkConnection connection = new NetworkConnection(client);
    await connection.ReadVarInt(); // packet length
    await connection.ReadVarInt(); // packet id
    
    var protocolVersion = await connection.ReadVarInt();
    var serverIp = await connection.ReadString(255);
    var serverPort = await connection.ReadUShort();
    var nextState = await connection.ReadVarInt();
    
    Console.WriteLine($"Protocol version: {protocolVersion} / Server IP: {serverIp} / Server Port: {serverPort} / Next State: {nextState}");

    if (nextState == 1)
    {
        await connection.ReadVarInt(); // packet length
        var packetId = await connection.ReadVarInt();
        if (packetId == 0)
        {
            var info = JsonSerializer.Serialize(serverInfo);
            await connection.WriteVarInt(info.Length + 1);
            await connection.WriteVarInt(0);
            Console.WriteLine(info);
            await connection.WriteString(info, 32767);
            await connection.Flush();
        }
        await connection.ReadVarInt();
        packetId = await connection.ReadVarInt();
        if (packetId == 1)
        {
            await connection.WriteVarInt(8 + 1); // length
            await connection.WriteVarInt(1);
            var v = await connection.ReadULong();
            await connection.WriteULong(v);
            await connection.Flush();
        }
    }
    
    // client.Close();
}
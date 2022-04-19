using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using MinecraftServer;
using MinecraftServer.Nbt;

Thread.CurrentThread.Name = "Socket Listener Thread";

FileStream output = new FileStream("test.nbt", FileMode.Create);
GZipStream gzip = new GZipStream(output, CompressionLevel.NoCompression);
NetworkConnection stream = new NetworkConnection(null, null, new BinaryWriter(gzip));


stream.WriteUByte(NbtTag.GetTag<NbtTagCompound>());
new NbtTagCompound()
    .Set("minecraft:dimension_type", 
        new NbtTagCompound()
            .SetString("type", "minecraft:dimension_type")
            .Set("value", 
                new NbtTagList<NbtTagCompound>()
                    .Add(
                        new NbtTagCompound()
                            .SetString("name", "minecraft:overworld")
                            .SetInteger("id", 0)
                            .Set("element",
                                new NbtTagCompound()
                                    .SetByte("piglin_safe", 0)
                                    .SetByte("natural", 1)
                                    .SetFloat("ambient_light", 1)
                                    .SetString("infiniburn", "")
                                    .SetByte("respawn_anchor_works", 0)
                                    .SetByte("has_skylight", 1)
                                    .SetByte("bed_works", 1)
                                    .SetString("effects", "minecraft:overworld")
                                    .SetByte("has_raids", 0)
                                    .SetInteger("min_y", 0)
                                    .SetInteger("height", 256)
                                    .SetInteger("logical_height", 256)
                                    .SetDouble("coordinate_scale", 1)
                                    .SetByte("ultrawarm", 0)
                                    .SetByte("has_ceiling", 0)
                            )
                    )
            )
    )
    .Write(stream);

gzip.Close();
output.Close();

var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(new IPEndPoint(IPAddress.Any, 25565));
server.Listen();
server.ReceiveTimeout = -1;
server.SendTimeout = -1;

var mcServer = new Server();

while (true)
{
    var client =  server.Accept();
    client.Blocking = false;
    mcServer.Connections.Add(new NetworkConnection(client, null, null));
}
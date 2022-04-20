using System.Text.Json.Serialization;

namespace MinecraftServer.Networking;

[JsonSerializable(typeof(ServerInfo))]
[JsonSerializable(typeof(ChatComponent))]
[JsonSerializable(typeof(GameProfile))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
public partial class SerializationContext : JsonSerializerContext
{
    
}
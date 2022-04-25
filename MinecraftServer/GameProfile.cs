namespace MinecraftServer;

public class GameProfile
{
    public string id { get; set; } = null!;
    public Guid Uuid => Guid.ParseExact(id, "N");
    public string name { get; set; } = null!;
    public List<Property> properties { get; set; } = null!;

    public class Property
    {
        public string name { get; set; } = null!;
        public string value { get; set; } = null!;
        public string signature { get; set; } = null!;
    }
}
namespace MinecraftServer;

public class GameProfile
{
    public string id { get; set; }
    public Guid Uuid => Guid.ParseExact(id, "N");
    public string name { get; set; }
    public List<Property> properties { get; set; }
    public class Property
    {
        public string name { get; set; }
        public string value { get; set; }
        public string signature { get; set; }
    }
}
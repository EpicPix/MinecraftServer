namespace MinecraftServer;

public record struct ServerInfo()
{
    public VersionInfo version { get; init; } = new();
    public PlayerInfo players { get; init; } = new();
    public DescriptionInfo description { get; init; } = new();
    public string favicon { get; } = "";

    public record struct VersionInfo()
    {
        public string name { get; set; } = "1.18.2";
        public int protocol { get; set; } = 758;
    };

    public record struct PlayerInfo()
    {
        public int max { get; set; } = 10;
        public int online { get; set; } = 0;
        public List<PlayerSampleInfo> sample { get; } = new();
    }

    public record struct PlayerSampleInfo
    {
        public string name { get; }
        public Guid id { get; }
    }

    public record struct DescriptionInfo()
    {
        public string text { get; set; } = "idk";
    }
    
}
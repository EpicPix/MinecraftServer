namespace MinecraftServer;

public record struct ServerInfo
{
    public VersionInfo version { get; init; }
    public PlayerInfo players { get; init; }
    public DescriptionInfo description { get; init; }
    public string favicon { get; }

    public ServerInfo()
    {
        version = new VersionInfo();
        players = new PlayerInfo();
        description = new DescriptionInfo();
        favicon = "";
    }
    
    public record struct VersionInfo
    {
        public string name { get; set; } = "1.18.2";
        public int protocol { get; set; } = 758;

        public VersionInfo() { }
    };

    public record struct PlayerInfo
    {
        public int max { get; set; } = 10;
        public int online { get; set; } = 0;
        public List<PlayerSampleInfo> sample { get; }

        public PlayerInfo()
        {
            sample = new List<PlayerSampleInfo>();
        }
    }

    public record struct PlayerSampleInfo
    {
        public string name { get; }
        public Guid id { get; }
    }

    public record struct DescriptionInfo
    {
        public string text { get; set; } = "idk";
        
        public DescriptionInfo() { }
    }
    
}
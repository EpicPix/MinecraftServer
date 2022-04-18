namespace MinecraftServer;

public struct ServerInfo
{

    public VersionInfo version { get; }
    public PlayerInfo players { get; }
    public DescriptionInfo description { get; }
    public string favicon { get; }

    public ServerInfo()
    {
        version = new VersionInfo();
        players = new PlayerInfo();
        description = new DescriptionInfo();
        favicon = "";
    }
    
    public struct VersionInfo
    {
        public string name { get; set; } = "1.18.2";
        public int protocol { get; set; } = 758;

        public VersionInfo() { }
    };

    public struct PlayerInfo
    {
        public int max { get; set; } = 10;
        public int online { get; set; } = 0;
        public List<PlayerSampleInfo> sample { get; }

        public PlayerInfo()
        {
            sample = new List<PlayerSampleInfo>();
        }
    }

    public struct PlayerSampleInfo
    {
        public string name { get; }
        public Guid id { get; }
    }

    public struct DescriptionInfo
    {
        public string text { get; set; } = "idk";
        
        public DescriptionInfo() { }
    }
    
}
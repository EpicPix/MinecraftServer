namespace MinecraftServer.Packets.Serverbound.Data;

public class CsPlayClientSettingsPacketData : PacketData
{

    public enum ChatModeEnum
    {
        Enabled = 0,
        CommandsOnly = 1,
        Hidden = 2
    }

    [Flags]
    public enum SkinPartsFlags
    {
        None = 0x00,
        Cape = 0x01,
        Jacket = 0x02,
        LeftSleeve = 0x04,
        RightSleeve = 0x08,
        LeftPantsLeg = 0x10,
        RightPantsLeg = 0x20,
        Hat = 0x40
    }

    public enum MainHandEnum
    {
        Left = 0,
        Right = 1
    }
    
    public string Locale { get; }
    public byte ViewDistance { get; }
    public ChatModeEnum ChatMode { get; }
    public bool ChatColors { get; }
    public SkinPartsFlags SkinParts { get; }
    public MainHandEnum MainHand { get; }
    public bool EnableTextFiltering { get; }
    public bool AllowServerListings { get; }

    public CsPlayClientSettingsPacketData(string locale, byte viewDistance, ChatModeEnum chatMode, bool chatColors, SkinPartsFlags skinParts, MainHandEnum mainHand, bool enableTextFiltering, bool allowServerListings)
    {
        Locale = locale;
        ViewDistance = viewDistance;
        ChatMode = chatMode;
        ChatColors = chatColors;
        SkinParts = skinParts;
        MainHand = mainHand;
        EnableTextFiltering = enableTextFiltering;
        AllowServerListings = allowServerListings;
    }

}
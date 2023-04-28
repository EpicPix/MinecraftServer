namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayPlayerInfoPacketData : PacketData
{
    public enum UpdateAction
    {
        AddPlayer = 0x01,
        InitializeChat = 0x02,
        UpdateGamemode = 0x04,
        UpdateListed = 0x08,
        UpdateLatency = 0x10,
        UpdateDisplayName = 0x20
    }

    public interface IAction
    {
        public Guid Uuid { get; set; }
    }

    public class AddPlayerAction : IAction
    {
        public Guid Uuid { get; set; }
        public string Username = null!;
        public GameProfile? Profile;
    }

    public class UpdateListedAction : IAction
    {
        public Guid Uuid { get; set; }
        public bool Listed;
    }

    public class UpdateGamemodeAction : IAction
    {
        public Guid Uuid { get; set; }
        public int Gamemode;
    }

    public class UpdateLatencyAction : IAction
    {
        public Guid Uuid { get; set; }
        public int Ping;
    }

    public class UpdateDisplayNameAction : IAction
    {
        public Guid Uuid { get; set; }
        public ChatComponent? DisplayName;
    }

    public UpdateAction Action;
    public IReadOnlyList<IAction> Actions;

    public ScPlayPlayerInfoPacketData(UpdateAction action, List<IAction> actions)
    {
        Action = action;
        Actions = actions.AsReadOnly();
    }

}
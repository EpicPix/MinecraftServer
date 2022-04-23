namespace MinecraftServer.Packets.Clientbound.Data;

public class ScPlayPlayerInfoPacketData : PacketData
{
    public enum UpdateAction
    {
        AddPlayer = 0,
        UpdateGamemode = 1,
        UpdateLatency = 2,
        UpdateDisplayName = 3,
        RemovePlayer = 4
    }

    public interface IAction
    {
        public Guid Uuid { get; set; }
    }

    public class AddPlayerAction : IAction
    {
        public Guid Uuid { get; set; }
        public string Username;
        public GameProfile? Profile;
        public int Gamemode;
        public int Ping;
        public ChatComponent? DisplayName;
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

    public class RemovePlayerAction : IAction
    {
        public Guid Uuid { get; set; }
    }

    public UpdateAction Action;
    public IReadOnlyList<IAction> Actions;

    public ScPlayPlayerInfoPacketData(UpdateAction action, List<IAction> actions)
    {
        Action = action;
        Actions = actions.AsReadOnly();
    }

}
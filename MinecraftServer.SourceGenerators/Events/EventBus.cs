namespace MinecraftServer.SourceGenerators.Events;

public class EventBus
{
    public virtual bool ShouldContinue()
    {
        return true;
    }
}

public enum EventBuses
{
    Packet = 0
}
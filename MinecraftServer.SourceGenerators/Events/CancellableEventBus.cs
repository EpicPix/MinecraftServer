namespace MinecraftServer.SourceGenerators.Events;

public class CancellableEventBus : EventBus
{
    private bool _isCancelled = false;
    public virtual void Cancel()
    {
        _isCancelled = true;
    }
    public new bool ShouldContinue()
    {
        return !_isCancelled;
    }
}
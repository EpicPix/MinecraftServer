// using MinecraftServer.SourceGeneration.Events;

using MinecraftServer.Packets.Serverbound.Handshake;
using MinecraftServer.SourceGeneration.Events;

namespace MinecraftServer;

[EventBus(1)]
public partial struct DataBus : IEventBus
{
    [EventHandler(1, typeof(CsHandshake), 100)]
    public static ValueTask thing(CsHandshake data, DataBus val)
    {
        return ValueTask.CompletedTask;
    }

    public static async ValueTask PostEventAsync<T>(T data, DataBus instance) {

        switch (data)
        {
            case CsHandshake c:
                if(instance.ShouldContinue()) await MinecraftServer.DataBus.thing(c, instance);
                if(instance.ShouldContinue()) await MinecraftServer.DataBus.thing(c, instance);
                if(instance.ShouldContinue()) await MinecraftServer.DataBus.thing(c, instance);
                if(instance.ShouldContinue()) await MinecraftServer.DataBus.thing(c, instance);
                break;
            case int d:
                return MinecraftServer.DataBus.thing(d, instance);
                break;
        }
    }

    public bool ShouldContinue()
    {
        throw new NotImplementedException();
    }
}